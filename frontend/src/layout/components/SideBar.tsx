import { useState, useEffect } from "react";
import { SideBarItem } from "../components/SideBarItem";
import NormalButton from "../components/NormalButton";
import { IconContent } from "../components/Icons";
import { useNavigate, useLocation } from "react-router-dom";
import LanguageSwitch from "../components/LanguageSwitch";
import { useTranslation } from "react-i18next";
import Icon from "./Icon";

export default function SideBar() {
  const { t } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  const [selected, setSelected] = useState<string | null>(null);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    setSelected(location.pathname);
  }, [location.pathname]);

  const navItems = [
    { label: t("home"), path: "/Home" },
    { label: t("courses"), path: "/Courses" },
    { label: t("attendances"), path: "/Attendances" },
    { label: t("statistics"), path: "/Statistics" },
  ];

  return (
    <div className="flex flex-row mt-20 max-md:mb-5 overflow-y-auto">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 md:hidden bg-button-dark border-main-blue border-2 absolute top-15 right-5 z-50 rounded-2xl "
      >
        <Icon iconContent={IconContent["menu-icon"]} size={20} color={"#E8EEF1"} strokeWidth={2} />
      </button>
      <div
        className={`fixed top-0 left-0 h-screen bg-main-dark text-white md:w-90 max-md:w-screen transform transition-transform duration-300 ease-in-out z-40 ${
          isOpen ? "translate-x-0" : "-translate-x-full"
        } md:translate-x-0 md:block`}
      >
        <div className="p-5 mt-10 flex items-center gap-2">
          <img src="../assets/logo/white-on-transparent.png" alt="EduCode Logo" className="md:h-12 max-md:h-12 w-auto" />
        </div>

        <nav className="flex flex-col mt-5">
          {navItems.map((item, index) => (
            <div key={item.path}>
              <SideBarItem
                label={item.label}
                isSelected={selected === item.path}
                onClick={() => {
                  setSelected(item.path);
                  setIsOpen(false);
                  navigate(item.path);
                }}
              />
              {index !== navItems.length - 1 && <div className="w-55 h-[2px] bg-main-blue"></div>}
            </div>
          ))}
        </nav>
        <div className="absolute bottom-0 md:py-10 max-md:py-20 w-full flex justify-center gap-2 px-2">
          <NormalButton
            text={t("settings")}
            onClick={() => {
              navigate(`/Settings`);
              setIsOpen(false);
              setSelected(null);
            }}
          />
          <LanguageSwitch />
        </div>
      </div>
    </div>
  );
}
