import TerminalInput from "@/app/TerminalInput";
import LegendDropdown from "@/app/DropdownLegend";

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-between p-24">
        <LegendDropdown/>
        <TerminalInput/>
    </main>
  );
}
