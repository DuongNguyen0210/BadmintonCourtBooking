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
    if (error.response?.status === 401) {
      return 'Phiên đăng nhập không hợp lệ hoặc cookie chưa được gửi. Hãy đăng nhập lại rồi thử tạo bài đăng.'
    }

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
    <main className="page page-narrow">
      <div className="mb-5">
        <Link className="text-sm font-semibold text-emerald-700 hover:text-emerald-800" to="/feed">
          Quay lại bảng tin
        </Link>
        <h1 className="page-title mt-3">Tạo bài đăng</h1>
        <p className="page-subtitle">Điền thông tin sân, thời gian chơi và số slot cần tuyển.</p>
      </div>

      <section className="panel panel-pad">
        {error ? <div className="alert-error mb-5">{error}</div> : null}

        <PlaySessionPostForm
          isSubmitting={isSubmitting}
          onSubmit={handleSubmit}
          submitLabel="Tạo bài đăng"
        />
      </section>
    </main>
  )
}
