import axios from 'axios'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getPlaySessionPosts } from '../api/playSessions'
import { PlaySessionCard } from '../components/play-sessions/PlaySessionCard'
import type { PlaySessionPostListItem } from '../types/playSession'

function getErrorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data

    if (typeof data === 'object' && data !== null && 'message' in data) {
      return String(data.message)
    }
  }

  return 'Không tải được bảng tin. Vui lòng thử lại.'
}

export function FeedPage() {
  const [posts, setPosts] = useState<PlaySessionPostListItem[]>([])
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    let isMounted = true

    async function loadPosts() {
      try {
        const data = await getPlaySessionPosts()
        if (isMounted) {
          setPosts(data)
        }
      } catch (feedError) {
        if (isMounted) {
          setError(getErrorMessage(feedError))
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    void loadPosts()

    return () => {
      isMounted = false
    }
  }, [])

  return (
    <main className="mx-auto w-full max-w-3xl flex-1 px-4 py-6 sm:px-6 lg:py-8">
      <div className="mb-5 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-gray-950">Bảng tin cầu lông</h1>
          <p className="mt-1 text-sm text-gray-600">Các kèo vãng lai còn tuyển người.</p>
        </div>

        <Link
          className="inline-flex items-center justify-center rounded bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2"
          to="/play-sessions/create"
        >
          Tạo bài đăng
        </Link>
      </div>

      {isLoading ? (
        <div className="space-y-4" aria-busy="true" aria-label="Đang tải bảng tin">
          {[0, 1, 2].map((item) => (
            <div key={item} className="h-56 rounded border border-gray-200 bg-white p-5">
              <div className="h-4 w-32 rounded bg-gray-100" />
              <div className="mt-4 h-6 w-2/3 rounded bg-gray-100" />
              <div className="mt-6 grid grid-cols-1 gap-3 sm:grid-cols-2">
                <div className="h-14 rounded bg-gray-100" />
                <div className="h-14 rounded bg-gray-100" />
              </div>
            </div>
          ))}
        </div>
      ) : null}

      {!isLoading && error ? (
        <div className="rounded border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
          {error}
        </div>
      ) : null}

      {!isLoading && !error && posts.length === 0 ? (
        <section className="rounded border border-gray-200 bg-white p-8 text-center">
          <h2 className="text-lg font-semibold text-gray-950">Chưa có bài đăng phù hợp</h2>
          <p className="mt-2 text-sm text-gray-600">
            Các bài đã đủ người hoặc quá giờ chơi sẽ không xuất hiện ở đây.
          </p>
          <Link
            className="mt-5 inline-flex items-center justify-center rounded bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2"
            to="/play-sessions/create"
          >
            Tạo bài đăng đầu tiên
          </Link>
        </section>
      ) : null}

      {!isLoading && !error && posts.length > 0 ? (
        <div className="space-y-4">
          {posts.map((post) => (
            <PlaySessionCard key={post.id} post={post} />
          ))}
        </div>
      ) : null}
    </main>
  )
}
