import { Route, Routes } from 'react-router'

import { SiteHeader } from '@/components/site-header'
import { AdminPage } from '@/pages/AdminPage'
import { BookPage } from '@/pages/BookPage'
import { EventTypePage } from '@/pages/EventTypePage'
import { HomePage } from '@/pages/HomePage'
import { MeetingsPage } from '@/pages/MeetingsPage'

export default function App() {
  return (
    <div className="flex min-h-dvh flex-col">
      <SiteHeader />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/book" element={<BookPage />} />
        <Route path="/book/:typeId" element={<EventTypePage />} />
        <Route path="/admin" element={<AdminPage />} />
        <Route path="/admin/meetings" element={<MeetingsPage />} />
      </Routes>
    </div>
  )
}
