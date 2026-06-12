import { useAuth } from '../contexts/AuthContext'

export function DashboardPage() {
  const { user } = useAuth()
  const displayName = user?.fullName || user?.email || 'there'

  return (
    <main className="mx-auto w-full max-w-5xl px-6 py-10">
      <section className="rounded border border-gray-200 bg-white p-6">
        <p className="text-sm font-medium uppercase tracking-wide text-emerald-700">
          Dashboard
        </p>
        <h1 className="mt-3 text-2xl font-semibold text-gray-950">
          Welcome, {displayName}
        </h1>
        <p className="mt-3 max-w-2xl text-sm leading-6 text-gray-600">
          Your authentication session is active. This page is protected and only
          available after login.
        </p>
      </section>
    </main>
  )
}
