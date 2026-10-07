'use strict';

// ---------- Справочники подписей ----------
const L = {
  VehicleType: { Bus: 'Автобус', Minibus: 'Микроавтобус', Truck: 'Грузовой', Car: 'Легковой' },
  Ownership: { Own: 'Собственный', Contracted: 'Привлечённый' },
  VehicleStatus: { Ready: 'Исправен', Maintenance: 'На ТО / в ремонте', Unavailable: 'Недоступен' },
  RouteKind: { Main: 'Основной', Reserve: 'Запасной' }
};

// ---------- Утилиты ----------
function esc(v) {
  return String(v ?? '').replace(/[&<>"']/g, c =>
    ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}
const dash = v => (v === null || v === undefined || v === '' ? '—' : esc(v));
const $ = id => document.getElementById(id);

function showError(msg) {
  const box = $('errorBox');
  box.textContent = msg;
  box.classList.toggle('hidden', !msg);
  if (msg) window.scrollTo({ top: 0, behavior: 'smooth' });
}

async function api(method, url, body, isForm) {
  const opt = { method, headers: {} };
  if (isForm) {
    opt.body = body;
  } else if (body !== undefined) {
    opt.headers['Content-Type'] = 'application/json';
    opt.body = JSON.stringify(body);
  }
  const res = await fetch(url, opt);
  if (!res.ok) {
    let msg = res.status + ' ' + res.statusText;
    try {
      const j = await res.json();
      if (j.error) msg = j.error;
      else if (j.errors) msg = Object.values(j.errors).flat().join('; ');
      else if (j.title) msg = j.title;
    } catch { /* тело не JSON */ }
    throw new Error(msg);
  }
  if (res.status === 204) return null;
  return (res.headers.get('content-type') || '').includes('json') ? res.json() : null;
}

// Кэш справочников для подписей и выпадающих списков
const cache = { vehicles: [], points: [], routes: [], employees: [] };
const nameOf = (list, id) => {
  const x = list.find(i => i.id === id);
  return x ? x.name : '—';
};

// ---------- Универсальная форма ----------
let formSubmit = null;

function openForm(title, fields, values, onSubmit) {
  $('formTitle').textContent = title;
  $('formError').classList.add('hidden');
  $('formGrid').innerHTML = fields.map(f => {
    const v = values?.[f.name];
    const cls = f.full ? ' full' : '';
    if (f.type === 'checkbox') {
      return `<label class="check${cls}"><input type="checkbox" data-f="${f.name}" ${v ? 'checked' : ''}/> ${esc(f.label)}</label>`;
    }
    let input;
    if (f.type === 'select') {
      const opts = f.options().map(o =>
        `<option value="${esc(o.value)}" ${String(v ?? '') === String(o.value) ? 'selected' : ''}>${esc(o.label)}</option>`).join('');
      input = `<select data-f="${f.name}">${opts}</select>`;
    } else if (f.type === 'textarea') {
      input = `<textarea data-f="${f.name}" maxlength="${f.max || 1000}">${esc(v ?? '')}</textarea>`;
    } else {
      const extra = f.type === 'number' ? ` step="${f.step || 1}" min="${f.min ?? 0}"` : ` maxlength="${f.max || 200}"`;
      input = `<input type="${f.type}" data-f="${f.name}" value="${esc(v ?? '')}"${extra}${f.required ? ' required' : ''}/>`;
    }
    return `<label class="${cls.trim()}">${esc(f.label)}${f.required ? '*' : ''}${input}</label>`;
  }).join('');

  formSubmit = async () => {
    const data = {};
    for (const f of fields) {
      const el = document.querySelector(`[data-f="${f.name}"]`);
      if (f.type === 'checkbox') data[f.name] = el.checked;
      else if (f.type === 'number') data[f.name] = el.value === '' ? 0 : Number(el.value);
      else if (f.type === 'select' && f.numeric) data[f.name] = el.value === '' ? null : Number(el.value);
      else data[f.name] = el.value === '' ? (f.required ? '' : null) : el.value;
    }
    await onSubmit(data);
  };
  $('formOverlay').style.display = 'flex';
}

function closeForm() { $('formOverlay').style.display = 'none'; formSubmit = null; }

$('btnCancel').addEventListener('click', closeForm);
$('entityForm').addEventListener('submit', async e => {
  e.preventDefault();
  try {
    await formSubmit();
    closeForm();
  } catch (err) {
    const box = $('formError');
    box.textContent = err.message;
    box.classList.remove('hidden');
  }
});

// ---------- Описание сущностей ----------
const enumOptions = (dict, empty) => () =>
  [...(empty ? [{ value: '', label: '—' }] : []), ...Object.entries(dict).map(([value, label]) => ({ value, label }))];

const pointOptions = () => [{ value: '', label: '—' }, ...cache.points.map(p => ({ value: p.id, label: p.name }))];
const vehicleOptions = () => [{ value: '', label: '—' }, ...cache.vehicles.map(v => ({ value: v.id, label: `${v.name} (${v.capacity} мест)` }))];

const entities = {
  employees: {
    url: '/api/employees', addLabel: 'Добавить сотрудника', formTitle: 'сотрудника',
    columns: [
      ['ФИО', e => esc(e.fullName)],
      ['Подразделение', e => dash(e.department)],
      ['Должность', e => dash(e.position)],
      ['Смена', e => e.shift],
      ['Эвакуация', e => e.isSubjectToEvacuation
        ? '<span class="badge badge-Ready">Подлежит</span>'
        : '<span class="badge badge-gray">Остаётся</span>'],
      ['Маломобильный', e => (e.needsAssistance ? 'Да' : '')],
      ['Транспорт', e => (e.vehicleId ? esc(nameOf(cache.vehicles, e.vehicleId)) : '—')],
      ['Пункт размещения', e => (e.receptionPointId ? esc(nameOf(cache.points, e.receptionPointId)) : '—')]
    ],
    fields: [
      { name: 'fullName', label: 'ФИО', type: 'text', required: true, max: 150, full: true },
      { name: 'department', label: 'Подразделение', type: 'text', max: 150 },
      { name: 'position', label: 'Должность', type: 'text', max: 150 },
      { name: 'shift', label: 'Смена (1–5)', type: 'number', min: 1 },
      { name: 'phone', label: 'Телефон', type: 'text', max: 30 },
      { name: 'isSubjectToEvacuation', label: 'Подлежит эвакуации', type: 'checkbox' },
      { name: 'needsAssistance', label: 'Маломобильный (нужна помощь)', type: 'checkbox' }
    ],
    defaults: { shift: 1, isSubjectToEvacuation: true },
    extraActions: e => e.isSubjectToEvacuation ? `<button data-act="assign" data-id="${e.id}">Назначить</button>` : '',
    handleAction(act, item) {
      if (act !== 'assign') return false;
      openForm(`Назначение: ${item.fullName}`, [
        { name: 'vehicleId', label: 'Транспорт', type: 'select', numeric: true, options: vehicleOptions },
        { name: 'receptionPointId', label: 'Пункт размещения', type: 'select', numeric: true, options: pointOptions }
      ], item, async data => {
        await api('PATCH', `/api/employees/${item.id}/assignment`, data);
        await refreshAll();
      });
      return true;
    }
  },

  vehicles: {
    url: '/api/vehicles', addLabel: 'Добавить транспорт', formTitle: 'транспорт',
    rowClass: v => (v.isAvailableForEvacuation ? '' : 'unavailable'),
    columns: [
      ['Транспорт', v => esc(v.name)],
      ['Госномер', v => dash(v.plateNumber)],
      ['Тип', v => esc(L.VehicleType[v.type] || v.type)],
      ['Мест', v => v.capacity],
      ['Принадлежность', v => `<span class="badge badge-${esc(v.ownership)}">${esc(L.Ownership[v.ownership])}</span>`],
      ['Поставщик / договор', v => v.ownership === 'Contracted'
        ? `${dash(v.providerName)}<br><span class="small">${dash(v.contractNumber)}${v.contractValidUntil ? ' до ' + esc(v.contractValidUntil) : ''}${v.isContractExpired ? ' — <b>истёк</b>' : ''}</span>`
        : '—'],
      ['Водитель', v => `${dash(v.driverName)}<br><span class="small">${v.driverPhone ? esc(v.driverPhone) : ''}</span>`],
      ['Состояние', v => `<span class="badge badge-${esc(v.status)}">${esc(L.VehicleStatus[v.status])}</span>`]
    ],
    fields: [
      { name: 'name', label: 'Марка / наименование', type: 'text', required: true, max: 150, full: true },
      { name: 'plateNumber', label: 'Госномер', type: 'text', max: 20 },
      { name: 'type', label: 'Тип', type: 'select', options: enumOptions(L.VehicleType) },
      { name: 'capacity', label: 'Вместимость (мест)', type: 'number', min: 1 },
      { name: 'status', label: 'Состояние', type: 'select', options: enumOptions(L.VehicleStatus) },
      { name: 'ownership', label: 'Принадлежность', type: 'select', options: enumOptions(L.Ownership) },
      { name: 'providerName', label: 'Организация-поставщик (для привлечённого)', type: 'text', max: 200 },
      { name: 'contractNumber', label: 'Номер договора', type: 'text', max: 50 },
      { name: 'contractValidUntil', label: 'Договор действует до', type: 'date' },
      { name: 'driverName', label: 'Водитель', type: 'text', max: 150 },
      { name: 'driverPhone', label: 'Телефон водителя', type: 'text', max: 30 }
    ],
    defaults: { type: 'Bus', capacity: 20, status: 'Ready', ownership: 'Own' }
  },

  routes: {
    url: '/api/routes', addLabel: 'Добавить маршрут', formTitle: 'маршрут',
    columns: [
      ['Маршрут', r => esc(r.name)],
      ['Вид', r => `<span class="badge badge-${esc(r.kind)}">${esc(L.RouteKind[r.kind])}</span>`],
      ['Откуда', r => dash(r.startPoint)],
      ['Куда (пункт размещения)', r => (r.receptionPointId ? esc(nameOf(cache.points, r.receptionPointId)) : '—')],
      ['Км', r => r.distanceKm],
      ['Время, мин', r => r.travelTimeMinutes],
      ['Описание', r => `<span class="small">${dash(r.description)}</span>`],
      ['Карта', r => r.hasMap
        ? `<a href="/api/routes/${r.id}/map" target="_blank" rel="noopener">${esc(r.mapFileName || 'открыть')}</a>`
        : '<span class="badge badge-gray">нет</span>']
    ],
    fields: [
      { name: 'name', label: 'Название маршрута', type: 'text', required: true, max: 200, full: true },
      { name: 'kind', label: 'Вид', type: 'select', options: enumOptions(L.RouteKind) },
      { name: 'receptionPointId', label: 'Пункт размещения (куда)', type: 'select', numeric: true, options: pointOptions },
      { name: 'startPoint', label: 'Пункт отправления (откуда)', type: 'text', max: 300, full: true },
      { name: 'distanceKm', label: 'Протяжённость, км', type: 'number', step: 0.1 },
      { name: 'travelTimeMinutes', label: 'Время в пути, мин', type: 'number' },
      { name: 'description', label: 'Описание (участки, ориентиры)', type: 'textarea', max: 1000, full: true }
    ],
    defaults: { kind: 'Main' },
    extraActions: r => `<button data-act="upload" data-id="${r.id}">${r.hasMap ? 'Заменить карту' : 'Загрузить карту'}</button>` +
      (r.hasMap ? `<button data-act="delmap" data-id="${r.id}">Удалить карту</button>` : ''),
    handleAction(act, item) {
      if (act === 'upload') {
        const input = $('mapInput');
        input.value = '';
        input.onchange = async () => {
          if (!input.files.length) return;
          const fd = new FormData();
          fd.append('file', input.files[0]);
          try { await api('POST', `/api/routes/${item.id}/map`, fd, true); await refreshAll(); }
          catch (err) { showError(err.message); }
        };
        input.click();
        return true;
      }
      if (act === 'delmap') {
        if (!confirm('Удалить карту маршрута?')) return true;
        api('DELETE', `/api/routes/${item.id}/map`).then(refreshAll).catch(err => showError(err.message));
        return true;
      }
      return false;
    }
  },

  points: {
    url: '/api/reception-points', addLabel: 'Добавить пункт', formTitle: 'пункт размещения',
    columns: [
      ['Пункт', p => esc(p.name)],
      ['Адрес', p => esc(p.address)],
      ['Принимающая сторона', p => dash(p.hostOrganization)],
      ['Контакт', p => `${dash(p.contactPerson)}<br><span class="small">${p.contactPhone ? esc(p.contactPhone) : '<b>нет телефона</b>'}</span>`],
      ['Вместимость', p => p.capacity],
      ['Соглашение', p => dash(p.agreementNumber)]
    ],
    fields: [
      { name: 'name', label: 'Название', type: 'text', required: true, max: 200, full: true },
      { name: 'address', label: 'Адрес здания', type: 'text', required: true, max: 300, full: true },
      { name: 'hostOrganization', label: 'Принимающая организация', type: 'text', max: 200 },
      { name: 'agreementNumber', label: 'Номер соглашения', type: 'text', max: 50 },
      { name: 'contactPerson', label: 'Контактное лицо', type: 'text', max: 150 },
      { name: 'contactPhone', label: 'Телефон', type: 'text', max: 30 },
      { name: 'capacity', label: 'Вместимость (чел.)', type: 'number' },
      { name: 'notes', label: 'Примечание', type: 'textarea', max: 500, full: true }
    ],
    defaults: { capacity: 20 }
  }
};

const dataKey = { employees: 'employees', vehicles: 'vehicles', routes: 'routes', points: 'points' };

// ---------- Отрисовка CRUD-вкладок ----------
function renderEntity(key) {
  const cfg = entities[key];
  const items = cache[dataKey[key]];
  const rows = items.length === 0
    ? `<tr><td colspan="${cfg.columns.length + 1}" class="empty">Нет данных</td></tr>`
    : items.map(item => `<tr class="${cfg.rowClass ? cfg.rowClass(item) : ''}">` +
        cfg.columns.map(c => `<td>${c[1](item)}</td>`).join('') +
        `<td class="actions">${cfg.extraActions ? cfg.extraActions(item) : ''}` +
        `<button data-act="edit" data-id="${item.id}">Изменить</button>` +
        `<button data-act="delete" data-id="${item.id}">Удалить</button></td></tr>`).join('');

  $('tab-' + key).innerHTML =
    `<section class="toolbar"><button class="primary" data-act="add">+ ${esc(cfg.addLabel)}</button></section>` +
    `<table><thead><tr>${cfg.columns.map(c => `<th>${esc(c[0])}</th>`).join('')}<th></th></tr></thead><tbody>${rows}</tbody></table>`;
}

document.addEventListener('click', async e => {
  const btn = e.target.closest('button[data-act]');
  if (!btn) return;
  const section = btn.closest('section[id^="tab-"]');
  if (!section) return;
  const key = section.id.slice(4);
  const cfg = entities[key];
  if (!cfg) return;

  const act = btn.dataset.act;
  const item = btn.dataset.id ? cache[dataKey[key]].find(i => i.id === Number(btn.dataset.id)) : null;
  showError('');

  try {
    if (cfg.handleAction && item && cfg.handleAction(act, item)) return;

    if (act === 'add') {
      openForm('Новый ' + cfg.formTitle, cfg.fields, cfg.defaults, async data => {
        await api('POST', cfg.url, data);
        await refreshAll();
      });
    } else if (act === 'edit' && item) {
      openForm('Изменить ' + cfg.formTitle, cfg.fields, item, async data => {
        await api('PUT', `${cfg.url}/${item.id}`, data);
        await refreshAll();
      });
    } else if (act === 'delete' && item) {
      if (!confirm('Удалить запись?')) return;
      await api('DELETE', `${cfg.url}/${item.id}`);
      await refreshAll();
    }
  } catch (err) {
    showError(err.message);
  }
});

// ---------- Вкладка «Расчёты и план» ----------
function kpi(value, label, cls) {
  return `<div class="kpi ${cls || ''}"><div class="value">${esc(value)}</div><div class="label">${esc(label)}</div></div>`;
}

async function renderCalc() {
  const [s, plan] = await Promise.all([api('GET', '/api/evacuation/summary'), api('GET', '/api/evacuation/plan')]);

  $('warnings').innerHTML = s.warnings.length
    ? `<div class="warnings"><b>Требует внимания:</b><ul>${s.warnings.map(w => `<li>${esc(w)}</li>`).join('')}</ul></div>`
    : '<div class="warnings good">Замечаний нет: транспорта и мест размещения достаточно.</div>';

  const e = s.employees, t = s.transport, p = s.placement;
  $('kpis').innerHTML =
    kpi(`${e.subjectToEvacuation} из ${e.total}`, `Подлежит эвакуации (остаётся на объекте: ${e.stayingOnSite})`) +
    kpi(t.seatsTotal, `Мест в транспорте (свой ${t.seatsOwn} + привлечённый ${t.seatsContracted})`, t.seatsDeficit > 0 ? 'bad' : 'ok') +
    kpi(t.waves ?? '—', `Рейсов для вывоза всех${t.minVehiclesForOneWave != null ? `; за один рейс нужно машин: ${t.minVehiclesForOneWave}` : ''}`) +
    kpi(p.totalCapacity, `Мест размещения (пунктов: ${p.points})`, p.deficit > 0 ? 'bad' : 'ok') +
    kpi(`${s.routes.total} / ${s.routes.withMap}`, 'Маршрутов / с картой') +
    kpi(`${e.assignedToVehicle} / ${e.assignedToPoint}`, 'Назначено в транспорт / в пункты');

  $('shiftsBody').innerHTML = s.shifts.length
    ? s.shifts.map(r => `<tr><td>Смена ${r.shift}</td><td class="num">${r.total}</td><td class="num">${r.subjectToEvacuation}</td>` +
        `<td class="num">${r.needAssistance}</td>` +
        `<td class="num ${r.transportDeficit ? 'bad' : ''}">${r.transportDeficit}</td>` +
        `<td class="num ${r.placementDeficit ? 'bad' : ''}">${r.placementDeficit}</td></tr>`).join('') +
      `<tr><td><b>Итого</b></td><td class="num"><b>${e.total}</b></td><td class="num"><b>${e.subjectToEvacuation}</b></td>` +
        `<td class="num"><b>${e.needAssistance}</b></td><td class="num"><b>${t.seatsDeficit}</b></td><td class="num"><b>${p.deficit}</b></td></tr>`
    : '<tr><td colspan="6" class="empty">Нет сотрудников</td></tr>';

  $('planVehiclesBody').innerHTML = plan.vehicles.length
    ? plan.vehicles.map(v => `<tr class="${v.available ? '' : 'unavailable'}"><td>${esc(v.name)}</td><td>${dash(v.plateNumber)}</td>` +
        `<td>${dash(v.driverName)}</td><td class="num">${v.capacity}</td><td class="num">${v.assigned}</td>` +
        `<td class="small">${v.employees.map(x => esc(x.fullName) + (x.needsAssistance ? ' ♿' : '')).join(', ') || '—'}</td></tr>`).join('')
    : '<tr><td colspan="6" class="empty">Нет доступного транспорта</td></tr>';

  $('planPointsBody').innerHTML = plan.points.length
    ? plan.points.map(x => `<tr><td>${esc(x.name)}</td><td>${esc(x.address)}</td>` +
        `<td>${dash(x.contactPerson)}<br><span class="small">${dash(x.contactPhone)}</span></td>` +
        `<td class="num">${x.capacity}</td><td class="num">${x.assigned}</td></tr>`).join('')
    : '<tr><td colspan="5" class="empty">Нет пунктов размещения</td></tr>';

  $('unassignedBody').innerHTML = plan.unassigned.length
    ? plan.unassigned.map(x => `<tr><td>${esc(x.fullName)}</td><td>${x.shift}</td>` +
        `<td>${x.vehicleId ? esc(nameOf(cache.vehicles, x.vehicleId)) : '<b>не назначен</b>'}</td>` +
        `<td>${x.receptionPointId ? esc(nameOf(cache.points, x.receptionPointId)) : '<b>не назначен</b>'}</td></tr>`).join('')
    : '<tr><td colspan="4" class="empty">Все подлежащие эвакуации назначены</td></tr>';
}

$('btnAutoPlan').addEventListener('click', async () => {
  showError('');
  try {
    const shift = $('planShift').value;
    await api('POST', '/api/evacuation/plan/auto' + (shift ? `?shift=${encodeURIComponent(shift)}` : ''));
    await refreshAll();
  } catch (err) { showError(err.message); }
});

$('btnResetPlan').addEventListener('click', async () => {
  if (!confirm('Снять все назначения сотрудников?')) return;
  showError('');
  try { await api('DELETE', '/api/evacuation/plan'); await refreshAll(); }
  catch (err) { showError(err.message); }
});

$('btnRecalc').addEventListener('click', () => refreshAll());

// ---------- Вкладки и загрузка ----------
$('tabs').addEventListener('click', e => {
  const btn = e.target.closest('button[data-tab]');
  if (!btn) return;
  document.querySelectorAll('#tabs button').forEach(b => b.classList.toggle('active', b === btn));
  document.querySelectorAll('main > section').forEach(s => s.classList.add('hidden'));
  $('tab-' + btn.dataset.tab).classList.remove('hidden');
});

async function refreshAll() {
  try {
    showError('');
    const [employees, vehicles, routes, points] = await Promise.all([
      api('GET', '/api/employees'), api('GET', '/api/vehicles'),
      api('GET', '/api/routes'), api('GET', '/api/reception-points')
    ]);
    cache.employees = employees; cache.vehicles = vehicles; cache.routes = routes; cache.points = points;

    for (const key of Object.keys(entities)) renderEntity(key);
    await renderCalc();
  } catch (err) {
    showError('Не удалось загрузить данные: ' + err.message);
  }
}

refreshAll();
