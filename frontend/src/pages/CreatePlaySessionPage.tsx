import axios from 'axios'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { createPlaySessionPost } from '../api/playSessions'
import {
  PlaySessionPostForm,
  type PlaySessionPostFormValues,
} from '../components/play-sessions/PlaySessionPostForm'

function getErrorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data

    if (typeof data === 'object' && data !== null && 'message' in data) {
      return String(data.message)
    }

    if (typeof data === 'object' && data !== null && 'errors' in data) {
      return 'Dữ liệu bài đăng chưa hợp lệ.'
    }
  }

  return 'Không tạo được bài đăng. Vui lòng thử lại.'
}

export function CreatePlaySessionPage() {
  const navigate = useNavigate()
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(values: PlaySessionPostFormValues) {
    setError('')
    setIsSubmitting(true)

    try {
      const post = await createPlaySessionPost(values)
      navigate(`/play-sessions/${post.id}`, { replace: true })
    } catch (createError) {
      setError(getErrorMessage(createError))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="mx-auto w-full max-w-3xl flex-1 px-4 py-6 sm:px-6 lg:py-8">
      <div className="mb-5">
        <Link className="text-sm font-medium text-emerald-700 hover:text-emerald-800" to="/feed">
          Quay lại bảng tin
        </Link>
        <h1 className="mt-3 text-2xl font-semibold text-gray-950">Tạo bài đăng</h1>
      </div>

      <section className="rounded border border-gray-200 bg-white p-5">
        {error ? (
          <div className="mb-5 rounded border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
            {error}
          </div>
        ) : null}

        <PlaySessionPostForm
          isSubmitting={isSubmitting}
          onSubmit={handleSubmit}
          submitLabel="Tạo bài đăng"
        />
      </section>
    </main>
  )
}
