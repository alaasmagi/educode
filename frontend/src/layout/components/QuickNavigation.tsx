import NormalLink from "../components/Link";
import { useTranslation } from "react-i18next";

interface QuickNavigationItem {
  label: string;
  onClick: () => void;
}

interface QuickNavigationProperties {
  quickNavItemA: QuickNavigationItem;
  quickNavItemB: QuickNavigationItem;
}

const QuickNavigation: React.FC<QuickNavigationProperties> = ({
  quickNavItemA,
  quickNavItemB,
}) => {
  const { t } = useTranslation();
  return (
    <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl gap-4 p-6">
      <span className="text-2xl font-bold self-start">{t("quick-nav")}</span>
      <div className="flex flex-col items-start">
        <NormalLink
          text={quickNavItemA.label}
          onClick={quickNavItemA.onClick}
        />
        <NormalLink
          text={quickNavItemB.label}
          onClick={quickNavItemB.onClick}
        />
      </div>
    </div>
  );
};

export default QuickNavigation;
