import { useState, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Search, Download, ChevronUp, ChevronDown, GraduationCap, Plus, X, TrendingUp, Award, AlertTriangle, BarChart2 } from 'lucide-react'
import { gradesApi } from '../../services/gradesApi'
import { GradeRecord } from '../../types'

const gradeConfig: Record<string, { bg: string; text: string; ring: string; bar: string }> = {
  'A+': { bg: 'bg-emerald-50', text: 'text-emerald-600', ring: 'ring-emerald-200', bar: '#10b981' },
  'A':  { bg: 'bg-green-50',   text: 'text-green-600',   ring: 'ring-green-200',   bar: '#22c55e' },
  'B':  { bg: 'bg-blue-50',    text: 'text-blue-600',    ring: 'ring-blue-200',    bar: '#3b82f6' },
  'C':  { bg: 'bg-amber-50',   text: 'text-amber-600',   ring: 'ring-amber-200',   bar: '#f59e0b' },
  'D':  { bg: 'bg-orange-50',  text: 'text-orange-600',  ring: 'ring-orange-200',  bar: '#f97316' },
  'F':  { bg: 'bg-red-50',     text: 'text-red-500',     ring: 'ring-red-200',     bar: '#ef4444' },
}

const ALL_GRADES = ['All', 'A+', 'A', 'B', 'C', 'D', 'F']
type SortKey = 'score' | 'recordedAt' | 'courseName'
type SortDir = 'asc' | 'desc'

function ScoreRing({ score }: { score: number }) {
  const r = 18, c = 2 * Math.PI * r
  const cfg = score >= 90 ? gradeConfig['A+'] : score >= 80 ? gradeConfig['A'] : score >= 70 ? gradeConfig['B'] : score >= 60 ? gradeConfig['C'] : gradeConfig['F']
  return (
    <div className="flex items-center gap-3">
      <div className="relative w-12 h-12 flex-shrink-0">
        <svg width="48" height="48" viewBox="0 0 48 48">
          <circle cx="24" cy="24" r={r} fill="none" stroke="#f1f5f9" strokeWidth="3.5" />
          <circle cx="24" cy="24" r={r} fill="none" stroke={cfg.bar} strokeWidth="3.5"
            strokeDasharray={c} strokeDashoffset={c - (score / 100) * c}
            strokeLinecap="round" transform="rotate(-90 24 24)" />
        </svg>
        <span className="absolute inset-0 flex items-center justify-center text-xs font-bold text-gray-700">{score}</span>
      </div>
      <div className="w-16 h-1.5 bg-gray-100 rounded-full overflow-hidden">
        <div className="h-full rounded-full" style={{ width: `${score}%`, backgroundColor: cfg.bar }} />
      </div>
    </div>
  )
}

export default function Grades() {
  const queryClient = useQueryClient()
  const [studentId, setStudentId] = useState('STU001')
  const [search, setSearch] = useState('STU001')
  const [filterGrade, setFilterGrade] = useState('All')
  const [sortKey, setSortKey] = useState<SortKey>('recordedAt')
  const [sortDir, setSortDir] = useState<SortDir>('desc')
  const [courseSearch, setCourseSearch] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ studentId: '', courseId: '', courseName: '', score: '', recordedBy: '' })

  const { data: grades = [], isLoading } = useQuery<GradeRecord[]>({
    queryKey: ['grades', search],
    queryFn: () => gradesApi.getByStudent(search),
    enabled: !!search, retry: false,
  })

  const mutation = useMutation({
    mutationFn: gradesApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['grades'] })
      setForm({ studentId: '', courseId: '', courseName: '', score: '', recordedBy: '' })
      setShowForm(false)
    },
  })

  const filtered = useMemo(() => {
    let list = [...grades]
    if (filterGrade !== 'All') list = list.filter(g => g.grade === filterGrade)
    if (courseSearch) list = list.filter(g =>
      g.courseName.toLowerCase().includes(courseSearch.toLowerCase()) ||
      g.courseId.toLowerCase().includes(courseSearch.toLowerCase()))
    list.sort((a, b) => {
      let av: number | string = a[sortKey], bv: number | string = b[sortKey]
      if (sortKey === 'score') { av = +av; bv = +bv }
      return av < bv ? (sortDir === 'asc' ? -1 : 1) : av > bv ? (sortDir === 'asc' ? 1 : -1) : 0
    })
    return list
  }, [grades, filterGrade, courseSearch, sortKey, sortDir])

  const toggleSort = (key: SortKey) => {
    if (sortKey === key) setSortDir(d => d === 'asc' ? 'desc' : 'asc')
    else { setSortKey(key); setSortDir('desc') }
  }

  const exportCSV = () => {
    const rows = [['Student ID','Course ID','Course Name','Score','Grade','Recorded By','Date']]
    filtered.forEach(g => rows.push([g.studentId,g.courseId,g.courseName,String(g.score),g.grade,g.recordedBy,new Date(g.recordedAt).toLocaleDateString()]))
    const a = document.createElement('a')
    a.href = URL.createObjectURL(new Blob([rows.map(r=>r.join(',')).join('\n')],{type:'text/csv'}))
    a.download=`grades-${search}.csv`; a.click()
  }

  const avg = grades.length ? +(grades.reduce((s,g)=>s+g.score,0)/grades.length).toFixed(1) : 0
  const highest = grades.length ? Math.max(...grades.map(g=>g.score)) : 0
  const lowest = grades.length ? Math.min(...grades.map(g=>g.score)) : 0
  const passing = grades.filter(g=>g.score>=60).length

  const stats = [
    { label: 'Average Score', value: avg || '-', sub: 'overall performance', icon: TrendingUp, from: 'from-violet-500', to: 'to-indigo-600' },
    { label: 'Highest Score', value: highest || '-', sub: 'best result', icon: Award, from: 'from-emerald-400', to: 'to-teal-600' },
    { label: 'Needs Attention', value: lowest || '-', sub: 'lowest score', icon: AlertTriangle, from: 'from-rose-400', to: 'to-pink-600' },
    { label: 'Passing Rate', value: grades.length ? `${Math.round(passing/grades.length*100)}%` : '-', sub: `${passing}/${grades.length} students`, icon: BarChart2, from: 'from-amber-400', to: 'to-orange-500' },
  ]

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50/30 to-indigo-50/20 -m-6 p-6">
      <div className="max-w-6xl mx-auto space-y-6">

        {/* Header */}
        <div className="flex items-start justify-between pt-2">
          <div>
            <div className="flex items-center gap-3 mb-1">
              <div className="w-10 h-10 bg-gradient-to-br from-blue-600 to-violet-600 rounded-2xl flex items-center justify-center shadow-lg shadow-blue-200">
                <GraduationCap size={20} className="text-white" />
              </div>
              <h2 className="text-2xl font-bold text-gray-900 tracking-tight">Grade Records</h2>
            </div>
            <p className="text-gray-400 text-sm ml-13 pl-1">Academic performance tracker & analytics</p>
          </div>
          <div className="flex gap-2">
            <button onClick={() => setShowForm(f => !f)}
              className={`flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-semibold transition-all shadow-sm ${showForm ? 'bg-gray-100 text-gray-600 hover:bg-gray-200' : 'bg-gradient-to-r from-blue-600 to-violet-600 text-white hover:opacity-90 shadow-blue-200'}`}>
              {showForm ? <><X size={14}/> Cancel</> : <><Plus size={14}/> Add Grade</>}
            </button>
            {grades.length > 0 && (
              <button onClick={exportCSV} className="flex items-center gap-2 bg-white/80 backdrop-blur border border-gray-200 text-gray-600 px-4 py-2.5 rounded-xl text-sm font-semibold hover:bg-white transition-all">
                <Download size={14}/> Export
              </button>
            )}
          </div>
        </div>

        {/* Stat Cards */}
        {grades.length > 0 && (
          <div className="grid grid-cols-4 gap-4">
            {stats.map(({ label, value, sub, icon: Icon, from, to }) => (
              <div key={label} className={`bg-gradient-to-br ${from} ${to} rounded-2xl p-5 text-white shadow-lg relative overflow-hidden`}>
                <div className="absolute -right-4 -top-4 w-20 h-20 bg-white/10 rounded-full" />
                <div className="absolute -right-2 -bottom-6 w-16 h-16 bg-white/5 rounded-full" />
                <div className="relative">
                  <div className="flex items-center justify-between mb-3">
                    <p className="text-white/70 text-xs font-semibold uppercase tracking-widest">{label}</p>
                    <div className="w-8 h-8 bg-white/20 rounded-xl flex items-center justify-center">
                      <Icon size={15} className="text-white" />
                    </div>
                  </div>
                  <p className="text-3xl font-black tracking-tight">{value}</p>
                  <p className="text-white/60 text-xs mt-1">{sub}</p>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Add Grade Form */}
        {showForm && (
          <div className="bg-white/80 backdrop-blur-sm rounded-2xl border border-white shadow-xl p-6">
            <h3 className="text-sm font-bold text-gray-500 uppercase tracking-widest mb-5">New Grade Entry</h3>
            <form onSubmit={e => { e.preventDefault(); mutation.mutate({...form, score: parseFloat(form.score)}) }} className="grid grid-cols-2 gap-3">
              {[
                {key:'studentId',placeholder:'Student ID'},
                {key:'courseId',placeholder:'Course ID'},
                {key:'courseName',placeholder:'Course Name'},
                {key:'score',placeholder:'Score (0–100)',type:'number'},
                {key:'recordedBy',placeholder:'Recorded By'},
              ].map(({key,placeholder,type})=>(
                <input key={key} type={type||'text'} placeholder={placeholder}
                  value={form[key as keyof typeof form]}
                  onChange={e=>setForm(f=>({...f,[key]:e.target.value}))}
                  className="border border-gray-100 bg-gray-50 rounded-xl px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition-all placeholder:text-gray-300" required/>
              ))}
              <button type="submit" disabled={mutation.isPending}
                className="col-span-2 bg-gradient-to-r from-blue-600 to-violet-600 text-white rounded-xl py-3 text-sm font-bold hover:opacity-90 disabled:opacity-50 transition-all shadow-sm shadow-blue-200">
                {mutation.isPending ? 'Saving...' : '+ Save Grade Record'}
              </button>
            </form>
          </div>
        )}

        {/* Search Panel */}
        <div className="bg-white/80 backdrop-blur-sm rounded-2xl border border-white shadow-sm p-5">
          <div className="flex flex-wrap gap-3 mb-4">
            <div className="relative flex-1 min-w-52">
              <Search size={15} className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-300" />
              <input type="text" placeholder="Search by Student ID..."
                value={studentId} onChange={e=>setStudentId(e.target.value)}
                onKeyDown={e=>e.key==='Enter'&&setSearch(studentId)}
                className="w-full border border-gray-100 bg-gray-50 rounded-xl pl-10 pr-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition-all"/>
            </div>
            <input type="text" placeholder="Filter by course..."
              value={courseSearch} onChange={e=>setCourseSearch(e.target.value)}
              className="border border-gray-100 bg-gray-50 rounded-xl px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition-all w-48"/>
            <button onClick={()=>setSearch(studentId)}
              className="bg-gray-900 text-white px-6 py-3 rounded-xl text-sm font-bold hover:bg-gray-700 transition-all">
              Search
            </button>
          </div>

          {/* Grade filter pills */}
          <div className="flex gap-2 flex-wrap">
            {ALL_GRADES.map(g => {
              const count = g==='All' ? grades.length : grades.filter(gr=>gr.grade===g).length
              const cfg = gradeConfig[g]
              const active = filterGrade===g
              return (
                <button key={g} onClick={()=>setFilterGrade(g)}
                  className={`px-3.5 py-1.5 rounded-xl text-xs font-bold border transition-all ${
                    active
                      ? g==='All'
                        ? 'bg-gray-900 text-white border-gray-900 shadow-sm'
                        : `${cfg?.bg} ${cfg?.text} ring-1 ${cfg?.ring} border-transparent shadow-sm`
                      : 'bg-gray-50 text-gray-400 border-gray-100 hover:border-gray-200 hover:text-gray-600'
                  }`}>
                  {g} {count > 0 && <span className="ml-1 opacity-50 font-semibold">{count}</span>}
                </button>
              )
            })}
          </div>
        </div>

        {/* Table */}
        <div className="bg-white/90 backdrop-blur-sm rounded-2xl border border-white shadow-sm overflow-hidden">
          {isLoading ? (
            <div className="p-16 text-center">
              <div className="w-10 h-10 border-2 border-blue-500 border-t-transparent rounded-full animate-spin mx-auto mb-4"/>
              <p className="text-gray-400 text-sm font-medium">Fetching grade records...</p>
            </div>
          ) : filtered.length === 0 ? (
            <div className="p-16 text-center">
              <div className="w-16 h-16 bg-gradient-to-br from-gray-100 to-gray-50 rounded-2xl flex items-center justify-center mx-auto mb-4 shadow-inner">
                <GraduationCap size={24} className="text-gray-300"/>
              </div>
              <p className="text-gray-500 font-semibold text-sm">No grade records found</p>
              <p className="text-gray-300 text-xs mt-1">Try adjusting your search or filters</p>
            </div>
          ) : (
            <>
              <div className="px-6 py-3.5 border-b border-gray-50 flex items-center justify-between bg-gray-50/50">
                <p className="text-xs font-semibold text-gray-400">
                  <span className="text-gray-700">{filtered.length}</span> records {filtered.length < grades.length && <span className="text-gray-300">of {grades.length}</span>}
                </p>
              </div>
              <table className="w-full">
                <thead>
                  <tr className="border-b border-gray-50">
                    {[
                      {label:'Student', key:null},
                      {label:'Course', key:'courseName' as SortKey},
                      {label:'Score', key:'score' as SortKey},
                      {label:'Grade', key:null},
                      {label:'Recorded By', key:null},
                      {label:'Date', key:'recordedAt' as SortKey},
                    ].map(({label,key})=>(
                      <th key={label} onClick={()=>key&&toggleSort(key)}
                        className={`text-left px-6 py-4 text-xs font-bold text-gray-300 uppercase tracking-widest ${key?'cursor-pointer hover:text-gray-500 select-none':''}`}>
                        <span className="flex items-center gap-1">
                          {label}
                          {key && sortKey===key && (sortDir==='asc'?<ChevronUp size={11}/>:<ChevronDown size={11}/>)}
                        </span>
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {filtered.map((g, i) => {
                    const cfg = gradeConfig[g.grade] || { bg:'bg-gray-50', text:'text-gray-500', ring:'ring-gray-100', bar:'#94a3b8' }
                    return (
                      <tr key={g.id} className={`border-b border-gray-50/80 hover:bg-blue-50/30 transition-colors group ${i%2===0?'':'bg-gray-50/20'}`}>
                        <td className="px-6 py-4">
                          <div className="flex items-center gap-3">
                            <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-slate-700 to-slate-900 flex items-center justify-center text-white text-xs font-black shadow-sm group-hover:from-blue-600 group-hover:to-violet-600 transition-all">
                              {g.studentId.replace(/\D/g,'').slice(-2)||g.studentId.slice(0,2).toUpperCase()}
                            </div>
                            <span className="text-sm font-semibold text-gray-700">{g.studentId}</span>
                          </div>
                        </td>
                        <td className="px-6 py-4">
                          <p className="text-sm font-semibold text-gray-800">{g.courseName}</p>
                          <p className="text-xs text-gray-300 font-mono mt-0.5">{g.courseId}</p>
                        </td>
                        <td className="px-6 py-4"><ScoreRing score={g.score}/></td>
                        <td className="px-6 py-4">
                          <span className={`inline-flex items-center px-3 py-1 rounded-xl text-xs font-black ring-1 ${cfg.bg} ${cfg.text} ${cfg.ring}`}>
                            {g.grade}
                          </span>
                        </td>
                        <td className="px-6 py-4">
                          <div className="flex items-center gap-2">
                            <div className="w-6 h-6 rounded-lg bg-gray-100 flex items-center justify-center text-xs font-bold text-gray-500">
                              {g.recordedBy[0]?.toUpperCase()}
                            </div>
                            <span className="text-sm text-gray-500">{g.recordedBy}</span>
                          </div>
                        </td>
                        <td className="px-6 py-4 text-sm text-gray-300 font-medium">
                          {new Date(g.recordedAt).toLocaleDateString('en-US',{month:'short',day:'numeric',year:'numeric'})}
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
