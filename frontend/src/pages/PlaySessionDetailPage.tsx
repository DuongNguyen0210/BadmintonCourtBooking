import axios from 'axios'
import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { requestToJoin } from '../api/joinRequests'
import { getPlaySessionPost } from '../api/playSessions'
import type { JoinRequest } from '../types/joinRequest'
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
  const [joinRequest, setJoinRequest] = useState<JoinRequest | null>(null)
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [isRequesting, setIsRequesting] = useState(false)

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

  async function handleRequestToJoin() {
    if (!post) return

    setIsRequesting(true)
    setError('')

    try {
      const data = await requestToJoin(post.id)
      setJoinRequest(data)
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setIsRequesting(false)
    }
  }

  return (
    <main className="page page-narrow">
      <div className="mb-5">
        <Link className="text-sm font-semibold text-emerald-700 hover:text-emerald-800" to="/feed">
          Quay lại bảng tin
        </Link>
      </div>

      {isLoading ? (
        <section className="panel panel-pad" aria-busy="true">
          <div className="skeleton h-4 w-32" />
          <div className="skeleton mt-4 h-7 w-2/3" />
          <div className="mt-6 grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div className="skeleton h-16" />
            <div className="skeleton h-16" />
            <div className="skeleton h-16" />
            <div className="skeleton h-16" />
          </div>
        </section>
      ) : null}

      {!isLoading && error ? <div className="alert-error">{error}</div> : null}

      {!isLoading && !error && post ? (
        <article className="panel panel-pad">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div className="min-w-0">
              <p className="text-sm font-medium text-gray-500">{post.creatorName}</p>
              <h1 className="page-title mt-2">{post.title}</h1>
              <p className="mt-3 whitespace-pre-line text-sm leading-6 text-gray-700">
                {post.description || 'Chưa có mô tả.'}
              </p>
            </div>

            {post.canManage ? (
              <Link className="btn btn-secondary w-fit" to={`/play-sessions/${post.id}/edit`}>
                Sửa bài
              </Link>
            ) : (
              <button
                className="btn btn-primary w-fit"
                disabled={isRequesting || post.currentPlayers >= post.maxPlayers || joinRequest !== null}
                onClick={() => void handleRequestToJoin()}
                type="button"
              >
                {isRequesting
                  ? 'Đang gửi...'
                  : joinRequest
                    ? `Đã gửi: ${joinRequest.status}`
                    : 'Yêu cầu tham gia'}
              </button>
            )}
          </div>

          <dl className="mt-6 grid grid-cols-1 gap-3 text-sm sm:grid-cols-2">
            <div className="soft-panel px-3 py-2">
              <dt className="text-gray-500">Sân</dt>
              <dd className="mt-1 font-medium text-gray-900">{post.courtName}</dd>
            </div>

            <div className="soft-panel px-3 py-2">
              <dt className="text-gray-500">Địa chỉ</dt>
              <dd className="mt-1 font-medium text-gray-900">{post.courtAddress}</dd>
            </div>

            <div className="soft-panel px-3 py-2">
              <dt className="text-gray-500">Bắt đầu</dt>
              <dd className="mt-1 font-medium text-gray-900">
                {dateTimeFormatter.format(new Date(post.startTime))}
              </dd>
            </div>

            <div className="soft-panel px-3 py-2">
              <dt className="text-gray-500">Kết thúc</dt>
              <dd className="mt-1 font-medium text-gray-900">
                {dateTimeFormatter.format(new Date(post.endTime))}
              </dd>
            </div>

            <div className="soft-panel px-3 py-2">
              <dt className="text-gray-500">Chi phí mỗi người</dt>
              <dd className="mt-1 font-medium text-gray-900">
                {currencyFormatter.format(post.pricePerPlayerVnd)}
              </dd>
            </div>

            <div className="soft-panel px-3 py-2">
              <dt className="text-gray-500">Thành viên</dt>
              <dd className="mt-1 font-medium text-gray-900">
                {post.currentPlayers}/{post.maxPlayers}
              </dd>
            </div>

            {(post.malePlayers !== null || post.femalePlayers !== null) && (
              <div className="soft-panel px-3 py-2 sm:col-span-2">
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
