import axios from 'axios'
import { useEffect, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/useAuth'

type LocationState = {
  message?: string
}

function getErrorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data

    if (typeof data === 'object' && data !== null && 'message' in data) {
      return String(data.message)
    }
  }

  return 'Sign in failed. Check your email and password, then try again.'
}

export function LoginPage() {
  const { isAuthenticated, isLoading, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const state = location.state as LocationState | null

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(false)
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
    setIsSubmitting(true)

    try {
      await login({ email, password, rememberMe })
      navigate('/feed', { replace: true })
    } catch (loginError) {
      setError(getErrorMessage(loginError))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="page flex items-center justify-center">
      <section className="panel panel-pad w-full max-w-md">
        <div>
          <span className="badge badge-emerald">Welcome back</span>
          <h1 className="mt-3 text-2xl font-semibold text-gray-950">Sign in</h1>
          <p className="mt-2 text-sm text-gray-600">
            Continue to your feed, wallet, and join requests.
          </p>
        </div>

        {state?.message ? (
          <div className="alert-success mt-5">{state.message}</div>
        ) : null}

        {error ? <div className="alert-error mt-5">{error}</div> : null}

        <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
          <div>
            <label className="block text-sm font-medium text-gray-800" htmlFor="email">
              Email
            </label>
            <input
              autoComplete="email"
              className="input"
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
              autoComplete="current-password"
              className="input"
              id="password"
              onChange={(event) => setPassword(event.target.value)}
              required
              type="password"
              value={password}
            />
          </div>

          <label className="flex items-center gap-2 text-sm text-gray-700">
            <input
              checked={rememberMe}
              className="h-4 w-4 rounded border-gray-300 text-emerald-700 focus:ring-emerald-600"
              onChange={(event) => setRememberMe(event.target.checked)}
              type="checkbox"
            />
            Remember me
          </label>

          <button
            className="btn btn-primary w-full"
            disabled={isSubmitting}
            type="submit"
          >
            {isSubmitting ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        <p className="mt-5 text-center text-sm text-gray-600">
          No account yet?{' '}
          <Link className="font-medium text-emerald-700 hover:text-emerald-800" to="/register">
            Register
          </Link>
        </p>
      </section>
    </main>
  )
}
