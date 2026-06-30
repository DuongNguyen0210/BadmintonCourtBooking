import { Link } from 'react-router-dom'
import { useAuth } from '../contexts/useAuth'

export function DashboardPage() {
  const { user } = useAuth()
  const displayName = user?.fullName || user?.email || 'there'

  return (
    <main className="page page-wide">
      <section className="panel panel-pad">
        <span className="badge badge-emerald">Dashboard</span>
        <h1 className="mt-3 text-2xl font-semibold text-gray-950">Welcome, {displayName}</h1>
        <p className="mt-3 max-w-2xl text-sm leading-6 text-gray-600">
          Khu dashboard sau này sẽ dùng để quản lý tiền đi chơi, tiền thu từ host và
          thống kê hoạt động. Hiện tại bạn có thể đi tới feed, ví hoặc yêu cầu tham gia.
        </p>
        <div className="mt-6 flex flex-wrap gap-3">
          <Link className="btn btn-primary" to="/feed">
            Open feed
          </Link>
          <Link className="btn btn-secondary" to="/wallet">
            View wallet
          </Link>
        </div>
      </section>
    </main>
  )
}
