import axios from 'axios'
import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/useAuth'

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
    <main className="page flex items-center justify-center">
      <section className="panel panel-pad w-full max-w-md">
        <div>
          <span className="badge badge-emerald">New account</span>
          <h1 className="mt-3 text-2xl font-semibold text-gray-950">Create account</h1>
          <p className="mt-2 text-sm text-gray-600">
            Create a player account to request games and manage your wallet.
          </p>
        </div>

        {error ? <div className="alert-error mt-5">{error}</div> : null}

        <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
          <div>
            <label className="block text-sm font-medium text-gray-800" htmlFor="fullName">
              Full name
            </label>
            <input
              autoComplete="name"
              className="input"
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
              autoComplete="new-password"
              className="input"
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
              className="input"
              id="confirmPassword"
              minLength={8}
              onChange={(event) => setConfirmPassword(event.target.value)}
              required
              type="password"
              value={confirmPassword}
            />
          </div>

          <button
            className="btn btn-primary w-full"
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
