import { Link } from 'react-router-dom'
import type { PlaySessionPostListItem } from '../../types/playSession'

type PlaySessionCardProps = {
  post: PlaySessionPostListItem
}

const dateTimeFormatter = new Intl.DateTimeFormat('vi-VN', {
  dateStyle: 'medium',
  timeStyle: 'short',
})

const currencyFormatter = new Intl.NumberFormat('vi-VN', {
  currency: 'VND',
  maximumFractionDigits: 0,
  style: 'currency',
})

export function PlaySessionCard({ post }: PlaySessionCardProps) {
  const availableSlots = Math.max(post.maxPlayers - post.currentPlayers, 0)
  const startsAt = dateTimeFormatter.format(new Date(post.startTime))

  return (
    <article className="panel panel-pad">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0">
          <p className="text-sm font-medium text-gray-500">{post.creatorName}</p>
          <h2 className="mt-1 text-xl font-semibold text-gray-950">{post.title}</h2>
          <p className="mt-2 text-sm font-semibold text-gray-800">{post.courtName}</p>
          <p className="mt-1 text-sm leading-6 text-gray-600">{post.courtAddress}</p>
        </div>

        <span className="badge badge-emerald">Còn {availableSlots} slot</span>
      </div>

      <dl className="mt-5 grid grid-cols-1 gap-3 text-sm sm:grid-cols-2">
        <div className="soft-panel px-3 py-2">
          <dt className="text-gray-500">Khung giờ</dt>
          <dd className="mt-1 font-medium text-gray-900">{startsAt}</dd>
        </div>

        <div className="soft-panel px-3 py-2">
          <dt className="text-gray-500">Chi phí</dt>
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
          <div className="soft-panel px-3 py-2">
            <dt className="text-gray-500">Nam / nữ</dt>
            <dd className="mt-1 font-medium text-gray-900">
              {post.malePlayers !== null ? `${post.malePlayers} nam` : 'Ẩn nam'}
              {' · '}
              {post.femalePlayers !== null ? `${post.femalePlayers} nữ` : 'Ẩn nữ'}
            </dd>
          </div>
        )}
      </dl>

      <div className="mt-5 flex flex-wrap items-center gap-3">
        <Link className="btn btn-primary" to={`/play-sessions/${post.id}`}>
          Xem chi tiết
        </Link>

        {post.canManage ? (
          <Link className="btn btn-secondary" to={`/play-sessions/${post.id}/edit`}>
            Sửa bài
          </Link>
        ) : null}
      </div>
    </article>
  )
}
