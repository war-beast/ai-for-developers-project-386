import { Route, Routes } from 'react-router'

import { SiteHeader } from '@/components/site-header'
import { BookPage } from '@/pages/BookPage'
import { HomePage } from '@/pages/HomePage'

export default function App() {
  return (
    <div className="flex min-h-dvh flex-col">
      <SiteHeader />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/book" element={<BookPage />} />
      </Routes>
    </div>
  )
}
