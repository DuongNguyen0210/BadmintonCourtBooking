import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/useAuth'

function navClass({ isActive }: { isActive: boolean }) {
  return [
    'rounded-lg px-3 py-2 text-sm font-medium',
    isActive
      ? 'bg-emerald-50 text-emerald-800'
      : 'text-gray-600 hover:bg-gray-50 hover:text-gray-950',
  ].join(' ')
}

export function Navbar() {
  const { isAuthenticated, isLoading, logout, user } = useAuth()
  const navigate = useNavigate()

  async function handleLogout() {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <header className="sticky top-0 z-40 border-b border-gray-200/80 bg-white/95 backdrop-blur">
      <nav className="mx-auto flex min-h-16 w-full max-w-6xl flex-col gap-3 px-4 py-3 sm:px-6 lg:flex-row lg:items-center lg:justify-between">
        <Link className="flex min-w-0 items-center gap-3" to="/">
          <span className="grid h-9 w-9 shrink-0 place-items-center rounded-lg bg-emerald-700 text-sm font-bold text-white">
            BC
          </span>
          <span className="min-w-0">
            <span className="block truncate text-sm font-semibold text-gray-950">
              BadmintonCourtBooking
            </span>
            <span className="block truncate text-xs text-gray-500">Find courts and games</span>
          </span>
        </Link>

        <div className="flex flex-wrap items-center gap-1.5">
          <NavLink className={navClass} to="/">
            Home
          </NavLink>

          {isLoading ? (
            <span className="rounded-lg px-3 py-2 text-sm text-gray-500">Checking session...</span>
          ) : isAuthenticated ? (
            <>
              <NavLink className={navClass} to="/feed">
                Feed
              </NavLink>
              <NavLink className={navClass} to="/join-requests">
                Requests
              </NavLink>
              <NavLink className={navClass} to="/host/join-requests">
                Host
              </NavLink>
              <NavLink className={navClass} to="/wallet">
                Wallet
              </NavLink>
              <NavLink className={navClass} to="/notifications">
                Notifications
              </NavLink>
              <span className="ml-1 hidden max-w-44 truncate rounded-lg bg-gray-50 px-3 py-2 text-sm font-medium text-gray-700 md:block">
                {user?.fullName || user?.email}
              </span>
              <button
                className="btn btn-secondary min-h-9 px-3"
                onClick={handleLogout}
                type="button"
              >
                Logout
              </button>
            </>
          ) : (
            <>
              <NavLink className={navClass} to="/login">
                Login
              </NavLink>
              <Link className="btn btn-primary min-h-9 px-3" to="/register">
                Register
              </Link>
            </>
          )}
        </div>
      </nav>
    </header>
  )
}
