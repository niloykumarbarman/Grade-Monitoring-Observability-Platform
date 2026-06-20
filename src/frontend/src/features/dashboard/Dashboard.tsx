export default function Dashboard() {
  return (
    <div>
      <h2 className="text-2xl font-bold text-gray-800 mb-4">Dashboard</h2>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white rounded-lg shadow p-6">
          <p className="text-gray-500 text-sm">Total Alerts</p>
          <p className="text-3xl font-bold text-blue-600">0</p>
        </div>
        <div className="bg-white rounded-lg shadow p-6">
          <p className="text-gray-500 text-sm">Active Users</p>
          <p className="text-3xl font-bold text-green-600">0</p>
        </div>
        <div className="bg-white rounded-lg shadow p-6">
          <p className="text-gray-500 text-sm">System Health</p>
          <p className="text-3xl font-bold text-emerald-600">OK</p>
        </div>
      </div>
    </div>
  )
}
