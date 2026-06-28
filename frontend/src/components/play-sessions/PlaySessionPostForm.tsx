import { useMemo, useState } from 'react'
import type { CreatePlaySessionPostRequest } from '../../types/playSession'

export type PlaySessionPostFormValues = CreatePlaySessionPostRequest

type PlaySessionPostFormProps = {
  initialValues?: PlaySessionPostFormValues
  isSubmitting: boolean
  submitLabel: string
  onSubmit: (values: PlaySessionPostFormValues) => Promise<void>
}

function toInputDateTime(date: Date) {
  const pad = (value: number) => value.toString().padStart(2, '0')

  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(
    date.getHours(),
  )}:${pad(date.getMinutes())}`
}

function createDefaultValues(): PlaySessionPostFormValues {
  const startTime = new Date()
  startTime.setHours(startTime.getHours() + 2, 0, 0, 0)

  const endTime = new Date(startTime)
  endTime.setHours(endTime.getHours() + 2)

  return {
    title: '',
    description: '',
    courtName: '',
    courtAddress: '',
    startTime: toInputDateTime(startTime),
    endTime: toInputDateTime(endTime),
    pricePerPlayer: 0,
    maxPlayers: 4,
    currentPlayers: 1,
    malePlayers: 0,
    femalePlayers: 0,
    showMalePlayers: true,
    showFemalePlayers: true,
  }
}

function toApiDateTime(value: string) {
  return new Date(value).toISOString()
}

export function PlaySessionPostForm({
  initialValues,
  isSubmitting,
  submitLabel,
  onSubmit,
}: PlaySessionPostFormProps) {
  const defaults = useMemo(() => initialValues ?? createDefaultValues(), [initialValues])
  const [values, setValues] = useState(defaults)
  const [error, setError] = useState('')

  function updateField<K extends keyof PlaySessionPostFormValues>(
    field: K,
    value: PlaySessionPostFormValues[K],
  ) {
    setValues((current) => ({ ...current, [field]: value }))
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')

    if (new Date(values.startTime) >= new Date(values.endTime)) {
      setError('Giờ bắt đầu phải trước giờ kết thúc.')
      return
    }

    if (values.currentPlayers > values.maxPlayers) {
      setError('Số người hiện tại không được vượt quá số người tối đa.')
      return
    }

    if (values.malePlayers + values.femalePlayers > values.currentPlayers) {
      setError('Tổng số nam và nữ không được vượt quá số người hiện tại.')
      return
    }

    await onSubmit({
      ...values,
      title: values.title.trim(),
      description: values.description.trim(),
      courtName: values.courtName.trim(),
      courtAddress: values.courtAddress.trim(),
      startTime: toApiDateTime(values.startTime),
      endTime: toApiDateTime(values.endTime),
    })
  }

  return (
    <form className="space-y-5" onSubmit={handleSubmit}>
      {error ? (
        <div className="rounded border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
          {error}
        </div>
      ) : null}

      <div>
        <label className="block text-sm font-medium text-gray-800" htmlFor="title">
          Tiêu đề
        </label>
        <input
          className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
          id="title"
          maxLength={120}
          onChange={(event) => updateField('title', event.target.value)}
          required
          type="text"
          value={values.title}
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-800" htmlFor="description">
          Mô tả
        </label>
        <textarea
          className="mt-1 min-h-28 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
          id="description"
          maxLength={1000}
          onChange={(event) => updateField('description', event.target.value)}
          value={values.description}
        />
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="block text-sm font-medium text-gray-800" htmlFor="courtName">
            Tên sân
          </label>
          <input
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            id="courtName"
            maxLength={200}
            onChange={(event) => updateField('courtName', event.target.value)}
            required
            type="text"
            value={values.courtName}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-800" htmlFor="courtAddress">
            Địa chỉ sân
          </label>
          <input
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            id="courtAddress"
            maxLength={300}
            onChange={(event) => updateField('courtAddress', event.target.value)}
            required
            type="text"
            value={values.courtAddress}
          />
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="block text-sm font-medium text-gray-800" htmlFor="startTime">
            Giờ bắt đầu
          </label>
          <input
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            id="startTime"
            onChange={(event) => updateField('startTime', event.target.value)}
            required
            type="datetime-local"
            value={values.startTime}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-800" htmlFor="endTime">
            Giờ kết thúc
          </label>
          <input
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            id="endTime"
            onChange={(event) => updateField('endTime', event.target.value)}
            required
            type="datetime-local"
            value={values.endTime}
          />
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="block text-sm font-medium text-gray-800" htmlFor="pricePerPlayer">
            Chi phí mỗi người
          </label>
          <input
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            id="pricePerPlayer"
            min={0}
            onChange={(event) => updateField('pricePerPlayer', Number(event.target.value))}
            required
            type="number"
            value={values.pricePerPlayer}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-800" htmlFor="maxPlayers">
            Số người tối đa
          </label>
          <input
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            id="maxPlayers"
            min={1}
            onChange={(event) => updateField('maxPlayers', Number(event.target.value))}
            required
            type="number"
            value={values.maxPlayers}
          />
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div>
          <label className="block text-sm font-medium text-gray-800" htmlFor="currentPlayers">
            Số người hiện tại
          </label>
          <input
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            id="currentPlayers"
            min={0}
            onChange={(event) => updateField('currentPlayers', Number(event.target.value))}
            required
            type="number"
            value={values.currentPlayers}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-800" htmlFor="malePlayers">
            Số nam
          </label>
          <input
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            id="malePlayers"
            min={0}
            onChange={(event) => updateField('malePlayers', Number(event.target.value))}
            required
            type="number"
            value={values.malePlayers}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-800" htmlFor="femalePlayers">
            Số nữ
          </label>
          <input
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            id="femalePlayers"
            min={0}
            onChange={(event) => updateField('femalePlayers', Number(event.target.value))}
            required
            type="number"
            value={values.femalePlayers}
          />
        </div>
      </div>

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <label className="flex items-center gap-2 rounded border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-800">
          <input
            checked={values.showMalePlayers}
            className="h-4 w-4 rounded border-gray-300 text-emerald-700 focus:ring-emerald-600"
            onChange={(event) => updateField('showMalePlayers', event.target.checked)}
            type="checkbox"
          />
          Hiển thị số nam
        </label>

        <label className="flex items-center gap-2 rounded border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-800">
          <input
            checked={values.showFemalePlayers}
            className="h-4 w-4 rounded border-gray-300 text-emerald-700 focus:ring-emerald-600"
            onChange={(event) => updateField('showFemalePlayers', event.target.checked)}
            type="checkbox"
          />
          Hiển thị số nữ
        </label>
      </div>

      <button
        className="inline-flex w-full items-center justify-center rounded bg-emerald-700 px-4 py-2.5 text-sm font-medium text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2 disabled:cursor-not-allowed disabled:bg-gray-400 sm:w-auto"
        disabled={isSubmitting}
        type="submit"
      >
        {isSubmitting ? 'Đang lưu...' : submitLabel}
      </button>
    </form>
  )
}
