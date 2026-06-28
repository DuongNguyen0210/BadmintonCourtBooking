import axios from 'axios'
import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getPlaySessionPost } from '../api/playSessions'
import type { PlaySessionPostDetail } from '../types/playSession'

const dateTimeFormatter = new Intl.DateTimeFormat('vi-VN', {
  dateStyle: 'medium',
  timeStyle: 'short',
})

const currencyFormatter = new Intl.NumberFormat('vi-VN', {
  currency: 'VND',
  maximumFractionDigits: 0,
  style: 'currency',
})

function getErrorMessage(error: unknown) {
  if (axios.isAxiosError(error) && error.response?.status === 404) {
    return 'Không tìm thấy bài đăng.'
  }

  return 'Không tải được chi tiết bài đăng. Vui lòng thử lại.'
}

export function PlaySessionDetailPage() {
  const { id } = useParams()
  const [post, setPost] = useState<PlaySessionPostDetail | null>(null)
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)

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
      } catch (detailError) {
        if (isMounted) {
          setError(getErrorMessage(detailError))
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

  return (
    <main className="mx-auto w-full max-w-3xl flex-1 px-4 py-6 sm:px-6 lg:py-8">
      <div className="mb-5">
        <Link className="text-sm font-medium text-emerald-700 hover:text-emerald-800" to="/feed">
          Quay lại bảng tin
        </Link>
      </div>

      {isLoading ? (
        <section className="rounded border border-gray-200 bg-white p-5" aria-busy="true">
          <div className="h-4 w-32 rounded bg-gray-100" />
          <div className="mt-4 h-7 w-2/3 rounded bg-gray-100" />
          <div className="mt-6 grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div className="h-16 rounded bg-gray-100" />
            <div className="h-16 rounded bg-gray-100" />
            <div className="h-16 rounded bg-gray-100" />
            <div className="h-16 rounded bg-gray-100" />
          </div>
        </section>
      ) : null}

      {!isLoading && error ? (
        <div className="rounded border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
          {error}
        </div>
      ) : null}

      {!isLoading && !error && post ? (
        <article className="rounded border border-gray-200 bg-white p-5">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <p className="text-sm text-gray-500">{post.creatorName}</p>
              <h1 className="mt-2 text-2xl font-semibold text-gray-950">{post.title}</h1>
              <p className="mt-3 whitespace-pre-line text-sm leading-6 text-gray-700">
                {post.description || 'Chưa có mô tả.'}
              </p>
            </div>

            {post.canManage ? (
              <Link
                className="inline-flex w-fit items-center justify-center rounded border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-800 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2"
                to={`/play-sessions/${post.id}/edit`}
              >
                Sửa bài
              </Link>
            ) : null}
          </div>

          <dl className="mt-6 grid grid-cols-1 gap-3 text-sm sm:grid-cols-2">
            <div className="rounded border border-gray-100 bg-gray-50 px-3 py-2">
              <dt className="text-gray-500">Sân</dt>
              <dd className="mt-1 font-medium text-gray-900">{post.courtName}</dd>
            </div>

            <div className="rounded border border-gray-100 bg-gray-50 px-3 py-2">
              <dt className="text-gray-500">Địa chỉ</dt>
              <dd className="mt-1 font-medium text-gray-900">{post.courtAddress}</dd>
            </div>

            <div className="rounded border border-gray-100 bg-gray-50 px-3 py-2">
              <dt className="text-gray-500">Bắt đầu</dt>
              <dd className="mt-1 font-medium text-gray-900">
                {dateTimeFormatter.format(new Date(post.startTime))}
              </dd>
            </div>

            <div className="rounded border border-gray-100 bg-gray-50 px-3 py-2">
              <dt className="text-gray-500">Kết thúc</dt>
              <dd className="mt-1 font-medium text-gray-900">
                {dateTimeFormatter.format(new Date(post.endTime))}
              </dd>
            </div>

            <div className="rounded border border-gray-100 bg-gray-50 px-3 py-2">
              <dt className="text-gray-500">Chi phí mỗi người</dt>
              <dd className="mt-1 font-medium text-gray-900">
                {currencyFormatter.format(post.pricePerPlayer)}
              </dd>
            </div>

            <div className="rounded border border-gray-100 bg-gray-50 px-3 py-2">
              <dt className="text-gray-500">Thành viên</dt>
              <dd className="mt-1 font-medium text-gray-900">
                {post.currentPlayers}/{post.maxPlayers}
              </dd>
            </div>

            {(post.malePlayers !== null || post.femalePlayers !== null) && (
              <div className="rounded border border-gray-100 bg-gray-50 px-3 py-2 sm:col-span-2">
                <dt className="text-gray-500">Số nam / nữ</dt>
                <dd className="mt-1 font-medium text-gray-900">
                  {post.malePlayers !== null ? `${post.malePlayers} nam` : 'Ẩn số nam'}
                  {' · '}
                  {post.femalePlayers !== null ? `${post.femalePlayers} nữ` : 'Ẩn số nữ'}
                </dd>
              </div>
            )}
          </dl>
        </article>
      ) : null}
    </main>
  )
}
