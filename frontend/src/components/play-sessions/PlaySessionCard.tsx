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

  return (
    <article className="rounded border border-gray-200 bg-white p-5">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0">
          <p className="text-sm text-gray-500">{post.creatorName}</p>
          <h2 className="mt-1 text-xl font-semibold text-gray-950">{post.title}</h2>
          <p className="mt-2 text-sm font-medium text-gray-800">{post.courtName}</p>
          <p className="mt-1 text-sm leading-6 text-gray-600">{post.courtAddress}</p>
        </div>

        <span className="inline-flex w-fit rounded border border-emerald-200 bg-emerald-50 px-2.5 py-1 text-xs font-medium text-emerald-800">
          Còn {availableSlots} slot
        </span>
      </div>

      <dl className="mt-5 grid grid-cols-1 gap-3 text-sm sm:grid-cols-2">
        <div className="rounded border border-gray-100 bg-gray-50 px-3 py-2">
          <dt className="text-gray-500">Khung giờ</dt>
          <dd className="mt-1 font-medium text-gray-900">
            {dateTimeFormatter.format(new Date(post.startTime))}
          </dd>
        </div>

        <div className="rounded border border-gray-100 bg-gray-50 px-3 py-2">
          <dt className="text-gray-500">Chi phí</dt>
          <dd className="mt-1 font-medium text-gray-900">
            {currencyFormatter.format(post.pricePerPlayerVnd)}
          </dd>
        </div>

        <div className="rounded border border-gray-100 bg-gray-50 px-3 py-2">
          <dt className="text-gray-500">Thành viên</dt>
          <dd className="mt-1 font-medium text-gray-900">
            {post.currentPlayers}/{post.maxPlayers}
          </dd>
        </div>

        {(post.malePlayers !== null || post.femalePlayers !== null) && (
          <div className="rounded border border-gray-100 bg-gray-50 px-3 py-2">
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
        <Link
          className="inline-flex items-center justify-center rounded bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2"
          to={`/play-sessions/${post.id}`}
        >
          Xem chi tiết
        </Link>

        {post.canManage ? (
          <Link
            className="inline-flex items-center justify-center rounded border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-800 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2"
            to={`/play-sessions/${post.id}/edit`}
          >
            Sửa bài
          </Link>
        ) : null}
      </div>
    </article>
  )
}
