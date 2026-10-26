import { Route, Routes } from 'react-router'

import { BookPage } from '@/pages/BookPage'
import { HomePage } from '@/pages/HomePage'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/book" element={<BookPage />} />
    </Routes>
  )
}
