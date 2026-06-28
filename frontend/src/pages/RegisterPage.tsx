import axios from 'axios'
import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

function getErrorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data

    if (typeof data === 'object' && data !== null && 'message' in data) {
      return String(data.message)
    }
  }

  return 'Registration failed. Check your information, then try again.'
}

export function RegisterPage() {
  const { isAuthenticated, isLoading, register } = useAuth()
  const navigate = useNavigate()

  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      navigate('/feed', { replace: true })
    }
  }, [isAuthenticated, isLoading, navigate])

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')

    if (password !== confirmPassword) {
      setError('Passwords do not match.')
      return
    }

    setIsSubmitting(true)

    try {
      await register({ fullName, email, password, confirmPassword })
      navigate('/login', {
        replace: true,
        state: { message: 'Account created. Sign in to continue.' },
      })
    } catch (registerError) {
      setError(getErrorMessage(registerError))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="mx-auto flex min-h-screen w-full max-w-md items-center px-6 py-12">
      <section className="w-full rounded border border-gray-200 bg-white p-6">
        <div>
          <p className="text-sm font-medium uppercase tracking-wide text-emerald-700">
            BadmintonCourtBooking
          </p>
          <h1 className="mt-3 text-2xl font-semibold text-gray-950">Create account</h1>
          <p className="mt-2 text-sm text-gray-600">
            Register with your name, email, and password.
          </p>
        </div>

        {error ? (
          <div className="mt-5 rounded border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-800">
            {error}
          </div>
        ) : null}

        <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
          <div>
            <label className="block text-sm font-medium text-gray-800" htmlFor="fullName">
              Full name
            </label>
            <input
              autoComplete="name"
              className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
              id="fullName"
              maxLength={100}
              onChange={(event) => setFullName(event.target.value)}
              required
              type="text"
              value={fullName}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-800" htmlFor="email">
              Email
            </label>
            <input
              autoComplete="email"
              className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
              id="email"
              onChange={(event) => setEmail(event.target.value)}
              required
              type="email"
              value={email}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-800" htmlFor="password">
              Password
            </label>
            <input
              autoComplete="new-password"
              className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
              id="password"
              minLength={8}
              onChange={(event) => setPassword(event.target.value)}
              required
              type="password"
              value={password}
            />
          </div>

          <div>
            <label
              className="block text-sm font-medium text-gray-800"
              htmlFor="confirmPassword"
            >
              Confirm password
            </label>
            <input
              autoComplete="new-password"
              className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
              id="confirmPassword"
              minLength={8}
              onChange={(event) => setConfirmPassword(event.target.value)}
              required
              type="password"
              value={confirmPassword}
            />
          </div>

          <button
            className="w-full rounded bg-emerald-700 px-4 py-2.5 text-sm font-medium text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2 disabled:cursor-not-allowed disabled:bg-gray-400"
            disabled={isSubmitting}
            type="submit"
          >
            {isSubmitting ? 'Creating account...' : 'Create account'}
          </button>
        </form>

        <p className="mt-5 text-center text-sm text-gray-600">
          Already registered?{' '}
          <Link className="font-medium text-emerald-700 hover:text-emerald-800" to="/login">
            Sign in
          </Link>
        </p>
      </section>
    </main>
  )
}
