import { Link } from "react-router-dom";

export function HomePage() {
  return (
    <>
      <h1 className="text-3xl font-bold">Home</h1>
      <Link to="/learning-items">Learning Items</Link>
    </>
  );
}
