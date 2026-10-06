import { Route, Routes } from "react-router-dom";
import { HomePage } from "./pages/HomePage";
import { LearningItemsPage } from "./pages/LearningItemsPage";

function App() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/learning-items" element={<LearningItemsPage />} />
    </Routes>
  );
}

export default App;
