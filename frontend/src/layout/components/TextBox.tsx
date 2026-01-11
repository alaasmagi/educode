import { useState } from "react";
import { IconContent } from "./Icons";
import Icon from "./Icon";

interface TextBoxProperties {
  label: string;
  icon: keyof typeof IconContent;
  placeHolder?: string;
  isPassword?: boolean;
  disabled?: boolean;
  value?: string;
  autofocus?: boolean;
  onChange?: (text: string) => void;
}

export default function TextBox({
  label,
  placeHolder,
  icon,
  isPassword = false,
  value,
  disabled = false,
  autofocus = false,
  onChange,
}: TextBoxProperties) {
  const [isTextVisible, setIsTextVisible] = useState<boolean>(!isPassword);
  const [isFocused, setIsFocused] = useState<boolean>(autofocus);

  return (
    <div className="flex flex-col w-full">
      <label className="self-start">{label}</label>
      <div
        className={
          isFocused
            ? "flex flex-row w-max-screen items-center gap-1.5 border-[1px] py-2 px-1 rounded-xl border-main-blue"
            : "flex flex-row w-max-screen items-center gap-1.5 border-[1px] py-2 px-1 rounded-xl border-main-text"
        }
      >
        <Icon iconContent={IconContent[icon]} size={20} color={"#E8EEF1"} strokeWidth={2} />
        <input
          type={isTextVisible ? "text" : "password"}
          value={value}
          autoFocus={autofocus}
          onFocus={() => setIsFocused(true)}
          onBlur={() => setIsFocused(false)}
          placeholder={placeHolder}
          onChange={(e) => onChange && onChange(e.target.value)}
          disabled={disabled}
          className="outline-none text-main-text md:w-10/12 text-lg"
        />
        {isPassword && (
          <button onClick={() => setIsTextVisible((prev) => !prev)} className="pr-1 hover:cursor-pointer">
            {isTextVisible ? (
              <Icon iconContent={IconContent["visibility-on"]} size={20} color={"#E8EEF1"} strokeWidth={2} />
            ) : (
              <Icon iconContent={IconContent["visibility-off"]} size={20} color={"#E8EEF1"} strokeWidth={2} />
            )}
          </button>
        )}
      </div>
    </div>
  );
}
