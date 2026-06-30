import { Link, Navigate } from 'react-router-dom'
import { useAuth } from '../contexts/useAuth'

export function HomePage() {
  const { isAuthenticated } = useAuth()

  if (isAuthenticated) {
    return <Navigate replace to="/feed" />
  }

  return (
    <main className="page page-wide flex items-center">
      <section className="grid w-full gap-6 lg:grid-cols-[1.08fr_0.92fr] lg:items-center">
        <div className="py-8">
          <span className="badge badge-emerald">Badminton community</span>
          <h1 className="mt-4 max-w-3xl text-4xl font-semibold leading-tight text-gray-950 sm:text-5xl">
            Tìm kèo cầu lông vãng lai nhanh hơn, rõ slot hơn.
          </h1>
          <p className="mt-5 max-w-2xl text-base leading-7 text-gray-600">
            Theo dõi các bài tuyển người, xem giờ chơi, địa chỉ sân, chi phí và số
            slot còn trống trong một bảng tin gọn gàng.
          </p>

          <div className="mt-8 flex flex-col gap-3 sm:flex-row">
            <Link className="btn btn-primary" to="/register">
              Create account
            </Link>
            <Link className="btn btn-secondary" to="/login">
              Sign in
            </Link>
          </div>
        </div>

        <div className="panel panel-pad">
          <div className="flex items-start justify-between gap-4 border-b border-gray-100 pb-4">
            <div>
              <p className="text-sm font-medium text-gray-500">Live feed preview</p>
              <h2 className="mt-1 text-xl font-semibold text-gray-950">Evening doubles</h2>
            </div>
            <span className="badge badge-emerald">2 slots</span>
          </div>

          <div className="mt-5 grid gap-3 text-sm sm:grid-cols-2">
            <div className="soft-panel p-4">
              <p className="font-medium text-gray-500">Court</p>
              <p className="mt-1 font-semibold text-gray-950">District 7 Arena</p>
            </div>
            <div className="soft-panel p-4">
              <p className="font-medium text-gray-500">Fee</p>
              <p className="mt-1 font-semibold text-gray-950">80.000 VND</p>
            </div>
            <div className="soft-panel p-4">
              <p className="font-medium text-gray-500">Time</p>
              <p className="mt-1 font-semibold text-gray-950">19:00 - 21:00</p>
            </div>
            <div className="soft-panel p-4">
              <p className="font-medium text-gray-500">Players</p>
              <p className="mt-1 font-semibold text-gray-950">4 / 6</p>
            </div>
          </div>
        </div>
      </section>
    </main>
  )
}
