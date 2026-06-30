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
    <main className="page page-narrow">
      <div className="mb-5 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="page-title">Bảng tin cầu lông</h1>
          <p className="page-subtitle">Các kèo vãng lai còn tuyển người.</p>
        </div>

        <Link className="btn btn-primary" to="/play-sessions/create">
          Tạo bài đăng
        </Link>
      </div>

      {isLoading ? (
        <div className="space-y-4" aria-busy="true" aria-label="Đang tải bảng tin">
          {[0, 1, 2].map((item) => (
            <div key={item} className="panel panel-pad h-56">
              <div className="skeleton h-4 w-32" />
              <div className="skeleton mt-4 h-6 w-2/3" />
              <div className="mt-6 grid grid-cols-1 gap-3 sm:grid-cols-2">
                <div className="skeleton h-14" />
                <div className="skeleton h-14" />
              </div>
            </div>
          ))}
        </div>
      ) : null}

      {!isLoading && error ? <div className="alert-error">{error}</div> : null}

      {!isLoading && !error && posts.length === 0 ? (
        <section className="panel panel-pad text-center">
          <span className="badge badge-gray mx-auto">Empty feed</span>
          <h2 className="mt-3 text-lg font-semibold text-gray-950">Chưa có bài đăng phù hợp</h2>
          <p className="mt-2 text-sm text-gray-600">
            Các bài đã đủ người hoặc quá giờ chơi sẽ không xuất hiện ở đây.
          </p>
          <Link className="btn btn-primary mt-5" to="/play-sessions/create">
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
