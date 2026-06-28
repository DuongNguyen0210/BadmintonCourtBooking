import axios from 'axios'
import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  cancelPlaySessionPost,
  getPlaySessionPost,
  updatePlaySessionPost,
} from '../api/playSessions'
import {
  PlaySessionPostForm,
  type PlaySessionPostFormValues,
} from '../components/play-sessions/PlaySessionPostForm'
import type { PlaySessionPostDetail } from '../types/playSession'

function toInputDateTime(value: string) {
  const date = new Date(value)
  const pad = (part: number) => part.toString().padStart(2, '0')

  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(
    date.getHours(),
  )}:${pad(date.getMinutes())}`
}

function toFormValues(post: PlaySessionPostDetail): PlaySessionPostFormValues {
  return {
    title: post.title,
    description: post.description,
    courtName: post.courtName,
    courtAddress: post.courtAddress,
    startTime: toInputDateTime(post.startTime),
    endTime: toInputDateTime(post.endTime),
    pricePerPlayer: post.pricePerPlayer,
    maxPlayers: post.maxPlayers,
    currentPlayers: post.currentPlayers,
    malePlayers: post.malePlayers ?? 0,
    femalePlayers: post.femalePlayers ?? 0,
    showMalePlayers: post.showMalePlayers,
    showFemalePlayers: post.showFemalePlayers,
  }
}

function getErrorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    if (error.response?.status === 403) {
      return 'Bạn không có quyền sửa bài đăng này.'
    }

    if (error.response?.status === 404) {
      return 'Không tìm thấy bài đăng.'
    }

    const data = error.response?.data
    if (typeof data === 'object' && data !== null && 'message' in data) {
      return String(data.message)
    }
  }

  return 'Không lưu được bài đăng. Vui lòng thử lại.'
}

export function EditPlaySessionPage() {
  const { id } = useParams()
  const navigate = useNavigate()

  const [post, setPost] = useState<PlaySessionPostDetail | null>(null)
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isCancelling, setIsCancelling] = useState(false)

  useEffect(() => {
    let isMounted = true

    async function loadPost() {
      if (!id) {
        setError('Đường dẫn bài đăng không hợp lệ.')
        setIsLoading(false)
        return
      }

      try {
        const data = await getPlaySessionPost(id)
        if (isMounted) {
          setPost(data)
        }
      } catch (loadError) {
        if (isMounted) {
          setError(getErrorMessage(loadError))
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    void loadPost()

    return () => {
      isMounted = false
    }
  }, [id])

  async function handleSubmit(values: PlaySessionPostFormValues) {
    if (!id) {
      return
    }

    setError('')
    setIsSubmitting(true)

    try {
      const updatedPost = await updatePlaySessionPost(id, values)
      navigate(`/play-sessions/${updatedPost.id}`, { replace: true })
    } catch (updateError) {
      setError(getErrorMessage(updateError))
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleCancelPost() {
    if (!id) {
      return
    }

    const confirmed = window.confirm('Bạn muốn hủy bài đăng này?')
    if (!confirmed) {
      return
    }

    setError('')
    setIsCancelling(true)

    try {
      await cancelPlaySessionPost(id)
      navigate('/feed', { replace: true })
    } catch (cancelError) {
      setError(getErrorMessage(cancelError))
    } finally {
      setIsCancelling(false)
    }
  }

  return (
    <main className="mx-auto w-full max-w-3xl flex-1 px-4 py-6 sm:px-6 lg:py-8">
      <div className="mb-5">
        <Link className="text-sm font-medium text-emerald-700 hover:text-emerald-800" to="/feed">
          Quay lại bảng tin
        </Link>
        <h1 className="mt-3 text-2xl font-semibold text-gray-950">Sửa bài đăng</h1>
      </div>

      {isLoading ? (
        <section className="rounded border border-gray-200 bg-white p-5" aria-busy="true">
          <div className="h-8 w-1/2 rounded bg-gray-100" />
          <div className="mt-6 h-80 rounded bg-gray-100" />
        </section>
      ) : null}

      {!isLoading && error ? (
        <div className="mb-5 rounded border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
          {error}
        </div>
      ) : null}

      {!isLoading && post && !post.canManage ? (
        <section className="rounded border border-gray-200 bg-white p-6">
          <p className="text-sm text-gray-700">Bạn không có quyền sửa bài đăng này.</p>
        </section>
      ) : null}

      {!isLoading && post && post.canManage ? (
        <section className="rounded border border-gray-200 bg-white p-5">
          <PlaySessionPostForm
            initialValues={toFormValues(post)}
            isSubmitting={isSubmitting}
            onSubmit={handleSubmit}
            submitLabel="Lưu thay đổi"
          />

          <div className="mt-6 border-t border-gray-200 pt-5">
            <button
              className="inline-flex items-center justify-center rounded border border-red-300 bg-white px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-50 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:bg-gray-100 disabled:text-gray-400"
              disabled={isCancelling}
              onClick={handleCancelPost}
              type="button"
            >
              {isCancelling ? 'Đang hủy...' : 'Hủy bài đăng'}
            </button>
          </div>
        </section>
      ) : null}
    </main>
  )
}
