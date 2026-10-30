import { useEffect, useState } from 'react'
import { Landmark, LayoutDashboard, Users, CreditCard, ArrowLeftRight, LogOut, RefreshCw, Plus, Search, ShieldCheck, Wallet, Activity, Menu, X } from 'lucide-react'
import { api } from './api'

const money = value => new Intl.NumberFormat('en-LK', { style: 'currency', currency: 'LKR', maximumFractionDigits: 2 }).format(value || 0)

function Login({ onLogin }) {
  const [email, setEmail] = useState('admin@smartbank.local')
  const [password, setPassword] = useState('Admin@12345')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  async function submit(e) {
    e.preventDefault(); setError(''); setBusy(true)
    try { const result = await api('/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }); onLogin(result) }
    catch (err) { setError(err.message) } finally { setBusy(false) }
  }
  return <div className="login-page"><div className="login-card"><div className="brand-mark"><Landmark size={28}/></div><p className="eyebrow">WELCOME TO</p><h1>SmartBank</h1><p className="muted">Secure banking operations demo</p><form onSubmit={submit}><label>Email address</label><input type="email" value={email} onChange={e=>setEmail(e.target.value)} required/><label>Password</label><input type="password" value={password} onChange={e=>setPassword(e.target.value)} required/><button className="primary full" disabled={busy}>{busy?'Signing in…':'Sign in'}</button>{error&&<p className="error">{error}</p>}</form><div className="demo-hint"><strong>Local demo login</strong><br/>admin@smartbank.local / Admin@12345</div></div></div>
}

export default function App() {
  const [session, setSession] = useState(() => {
    const token = localStorage.getItem('smartbank_token')
    return token ? { token, fullName: localStorage.getItem('smartbank_name'), role: localStorage.getItem('smartbank_role'), email: localStorage.getItem('smartbank_email') } : null
  })
  const [page, setPage] = useState('Overview')
  const [summary, setSummary] = useState(null)
  const [accounts, setAccounts] = useState([])
  const [customers, setCustomers] = useState([])
  const [transactions, setTransactions] = useState([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [query, setQuery] = useState('')
  const [modal, setModal] = useState('')
  const [form, setForm] = useState({})
  const [notice, setNotice] = useState('')
  const isStaff = session?.role === 'Admin' || session?.role === 'BankOfficer'

  function login(data) {
    localStorage.setItem('smartbank_token', data.token); localStorage.setItem('smartbank_name', data.fullName)
    localStorage.setItem('smartbank_role', data.role); localStorage.setItem('smartbank_email', data.email)
    setSession(data)
  }
  function logout() { localStorage.clear(); setSession(null) }
  async function load() {
    setLoading(true); setError('')
    try {
      const [s, a] = await Promise.all([api('/dashboard/summary'), api('/accounts')])
      setSummary(s); setAccounts(a)
      if (isStaff) setCustomers(await api('/customers'))
      const accountId = a[0]?.id
      if (accountId && isStaff) setTransactions(await api(`/transactions/account/${accountId}`))
    } catch (e) { setError(e.message) } finally { setLoading(false) }
  }
  useEffect(() => { if (session) load() }, [session])
  if (!session) return <Login onLogin={login}/>
  const nav = [{name:'Overview',icon:LayoutDashboard},{name:'Accounts',icon:CreditCard},...(isStaff?[{name:'Customers',icon:Users},{name:'Transactions',icon:ArrowLeftRight}]:[])]
  const filteredAccounts = accounts.filter(a => `${a.accountNumber} ${a.customerName} ${a.accountType}`.toLowerCase().includes(query.toLowerCase()))
  const filteredCustomers = customers.filter(c => `${c.fullName} ${c.customerNumber} ${c.nicNumber}`.toLowerCase().includes(query.toLowerCase()))
  function openModal(type) { setForm({}); setNotice(''); setModal(type) }
  async function submit(e) {
    e.preventDefault(); setNotice('')
    try {
      if (modal === 'customer') await api('/customers', { method:'POST', body:JSON.stringify(form) })
      if (modal === 'account') await api('/accounts', { method:'POST', body:JSON.stringify({ ...form, customerId:Number(form.customerId) }) })
      if (modal === 'deposit' || modal === 'withdraw') await api(`/transactions/${modal}`, { method:'POST', body:JSON.stringify({ ...form, accountId:Number(form.accountId), amount:Number(form.amount) }) })
      if (modal === 'transfer') await api('/transactions/transfer', { method:'POST', body:JSON.stringify({ ...form, fromAccountId:Number(form.fromAccountId), toAccountId:Number(form.toAccountId), amount:Number(form.amount) }) })
      setModal(''); setNotice('Operation completed successfully.'); await load()
    } catch (e) { setNotice(e.message) }
  }
  return <div className="app-shell">
    <aside className="sidebar"><div className="brand"><div className="brand-mark"><Landmark/></div><div><strong>SmartBank</strong><small>Banking Management</small></div></div><div className="nav-label">WORKSPACE</div><nav>{nav.map(n=><button key={n.name} className={page===n.name?'nav-item active':'nav-item'} onClick={()=>{setPage(n.name);setQuery('')}}><n.icon size={18}/>{n.name}</button>)}</nav><div className="sidebar-bottom"><div className="secure"><ShieldCheck size={17}/><span>Protected demo session</span></div><button className="nav-item" onClick={logout}><LogOut size={18}/>Sign out</button></div></aside>
    <main className="main"><header className="topbar"><div><p className="eyebrow">SMARTBANK / WORKSPACE</p><h2>{page}</h2></div><div className="profile"><div className="avatar">{(session.fullName||'U').slice(0,1)}</div><div><strong>{session.fullName}</strong><small>{session.role}</small></div><button className="icon-btn" onClick={load} title="Refresh"><RefreshCw size={17}/></button></div></header>
      {error&&<div className="alert error">{error}</div>}{notice&&<div className="alert success">{notice}</div>}
      {loading&&<div className="loading">Refreshing workspace…</div>}
      {page==='Overview'&&<><div className="welcome"><div><p className="eyebrow light">YOUR BANKING OVERVIEW</p><h1>Good day, {session.fullName?.split(' ')[0]}.</h1><p>Here’s what’s happening across your SmartBank workspace.</p></div><div className="welcome-icon"><Landmark size={62}/></div></div><div className="stat-grid"><Stat icon={Users} label="Total customers" value={summary?.customerCount??'—'} tone="blue"/><Stat icon={CreditCard} label="Total accounts" value={summary?.totalAccounts??'—'} tone="purple"/><Stat icon={Wallet} label="Total account balances" value={money(summary?.totalBalance)} tone="green"/></div><section className="panel"><div className="panel-head"><div><h3>Accounts snapshot</h3><p className="muted">Recently available accounts</p></div><button className="text-btn" onClick={()=>setPage('Accounts')}>View accounts →</button></div><AccountTable rows={accounts.slice(0,5)}/></section></>}
      {page==='Accounts'&&<section className="panel"><div className="panel-head"><div><h3>Bank accounts</h3><p className="muted">Manage customer savings and current accounts</p></div>{isStaff&&<button className="primary" onClick={()=>openModal('account')}><Plus size={16}/> Open account</button>}</div><SearchBox value={query} onChange={setQuery}/><AccountTable rows={filteredAccounts}/></section>}
      {page==='Customers'&&<section className="panel"><div className="panel-head"><div><h3>Customers</h3><p className="muted">Customer records and account relationships</p></div><button className="primary" onClick={()=>openModal('customer')}><Plus size={16}/> Add customer</button></div><SearchBox value={query} onChange={setQuery}/><div className="table-wrap"><table><thead><tr><th>Customer</th><th>Customer ID</th><th>NIC</th><th>Email</th><th>Phone</th><th>Accounts</th></tr></thead><tbody>{filteredCustomers.map(c=><tr key={c.id}><td><strong>{c.fullName}</strong></td><td>{c.customerNumber}</td><td>{c.nicNumber}</td><td>{c.email}</td><td>{c.phone}</td><td>{c.accountCount}</td></tr>)}</tbody></table>{!filteredCustomers.length&&<Empty/>}</div></section>}
      {page==='Transactions'&&<><div className="action-grid"><Action title="Deposit" desc="Add funds to an account" onClick={()=>openModal('deposit')}/><Action title="Withdraw" desc="Withdraw funds from an account" onClick={()=>openModal('withdraw')}/><Action title="Transfer" desc="Move funds between accounts" onClick={()=>openModal('transfer')}/></div><section className="panel"><div className="panel-head"><div><h3>Recent transactions</h3><p className="muted">Showing activity for the first account</p></div></div><TransactionTable rows={transactions}/></section></>}
    </main>
    {modal&&<div className="modal-backdrop" onMouseDown={e=>e.target===e.currentTarget&&setModal('')}><div className="modal"><div className="panel-head"><div><p className="eyebrow">SMARTBANK OPERATIONS</p><h3>{({customer:'Add customer',account:'Open account',deposit:'Deposit funds',withdraw:'Withdraw funds',transfer:'Transfer funds'})[modal]}</h3></div><button className="icon-btn" onClick={()=>setModal('')}><X/></button></div><form onSubmit={submit}>
      {modal==='customer'&&<><Field label="Full name" value={form.fullName||''} onChange={v=>setForm({...form,fullName:v})} required/><Field label="NIC number" value={form.nicNumber||''} onChange={v=>setForm({...form,nicNumber:v})} required/><Field label="Email" type="email" value={form.email||''} onChange={v=>setForm({...form,email:v})} required/><Field label="Phone" value={form.phone||''} onChange={v=>setForm({...form,phone:v})} required/></>}
      {modal==='account'&&<><SelectField label="Customer" value={form.customerId||''} onChange={v=>setForm({...form,customerId:v})} options={customers.map(c=>({value:c.id,label:`${c.fullName} (${c.customerNumber})`}))}/><SelectField label="Account type" value={form.accountType||'Savings'} onChange={v=>setForm({...form,accountType:v})} options={[{value:'Savings',label:'Savings'},{value:'Current',label:'Current'}]}/></>}
      {(modal==='deposit'||modal==='withdraw')&&<><SelectField label="Account" value={form.accountId||''} onChange={v=>setForm({...form,accountId:v})} options={accounts.map(a=>({value:a.id,label:`${a.accountNumber} — ${money(a.balance)}`}))}/><Field label="Amount (LKR)" type="number" value={form.amount||''} onChange={v=>setForm({...form,amount:v})} required/><Field label="Description" value={form.description||''} onChange={v=>setForm({...form,description:v})}/></>}
      {modal==='transfer'&&<><SelectField label="From account" value={form.fromAccountId||''} onChange={v=>setForm({...form,fromAccountId:v})} options={accounts.map(a=>({value:a.id,label:a.accountNumber}))}/><SelectField label="To account" value={form.toAccountId||''} onChange={v=>setForm({...form,toAccountId:v})} options={accounts.map(a=>({value:a.id,label:a.accountNumber}))}/><Field label="Amount (LKR)" type="number" value={form.amount||''} onChange={v=>setForm({...form,amount:v})} required/><Field label="Description" value={form.description||''} onChange={v=>setForm({...form,description:v})}/></>}
      <button className="primary full" type="submit">Confirm operation</button></form></div></div>}
  </div>
}

function Stat({icon:Icon,label,value,tone}){return <div className="stat-card"><div className={`stat-icon ${tone}`}><Icon size={20}/></div><p>{label}</p><strong>{value}</strong></div>}
function SearchBox({value,onChange}){return <div className="searchbox"><Search size={17}/><input placeholder="Search records…" value={value} onChange={e=>onChange(e.target.value)}/></div>}
function AccountTable({rows}){return <div className="table-wrap"><table><thead><tr><th>Account number</th><th>Customer</th><th>Type</th><th>Balance</th><th>Status</th></tr></thead><tbody>{rows.map(a=><tr key={a.id}><td><strong>{a.accountNumber}</strong></td><td>{a.customerName}</td><td>{a.accountType}</td><td className="amount">{money(a.balance)}</td><td><span className={a.isActive?'status active':'status'}>{a.isActive?'Active':'Inactive'}</span></td></tr>)}</tbody></table>{!rows.length&&<Empty/>}</div>}
function TransactionTable({rows}){return <div className="table-wrap"><table><thead><tr><th>Reference</th><th>Type</th><th>Amount</th><th>Balance after</th><th>Date</th></tr></thead><tbody>{rows.map(t=><tr key={t.id}><td><strong>{t.reference}</strong></td><td>{t.type}</td><td className={t.amount<0?'negative':'positive'}>{money(t.amount)}</td><td>{money(t.balanceAfter)}</td><td>{new Date(t.createdAtUtc).toLocaleString()}</td></tr>)}</tbody></table>{!rows.length&&<Empty/>}</div>}
function Empty(){return <div className="empty"><Activity size={24}/><p>No records to display</p></div>}
function Action({title,desc,onClick}){return <button className="action-card" onClick={onClick}><span><ArrowLeftRight size={19}/></span><strong>{title}</strong><small>{desc}</small></button>}
function Field({label,value,onChange,type='text',required=false}){return <label className="field">{label}<input type={type} value={value} onChange={e=>onChange(e.target.value)} required={required} min={type==='number'?'0.01':undefined} step={type==='number'?'0.01':undefined}/></label>}
function SelectField({label,value,onChange,options}){return <label className="field">{label}<select value={value} onChange={e=>onChange(e.target.value)} required><option value="">Choose…</option>{options.map(o=><option key={o.value} value={o.value}>{o.label}</option>)}</select></label>}
