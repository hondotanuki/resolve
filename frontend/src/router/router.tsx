import { createBrowserRouter, Navigate } from "react-router-dom";
import { AppLayout } from "../components/layout/AppLayout";
import { LearningItemCreatePage } from "../pages/LearningItemCreatePage";
import { LearningItemDetailPage } from "../pages/LearningItemDetailPage";
import { LearningItemListPage } from "../pages/LearningItemListPage";

export const router = createBrowserRouter([
  {
    element: <AppLayout />,
    children: [
      {
        path: "/",
        element: <Navigate to="/learning-items" replace />,
      },
      {
        path: "/learning-items",
        element: <LearningItemListPage />,
      },
      {
        path: "/learning-items/new",
        element: <LearningItemCreatePage />,
      },
      {
        path: "/learning-items/:id",
        element: <LearningItemDetailPage />,
      },
    ],
  },
]);
