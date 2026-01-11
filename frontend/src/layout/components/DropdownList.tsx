import Icon from "./Icon";
import { IconContent } from "./Icons";

interface DropDownListProperties {
  icon: keyof typeof IconContent;
  options: { value: string; label: string }[];
  onChange: (event: React.ChangeEvent<HTMLSelectElement>) => void;
  label: string;
  value?: string;
}

const DropDownList: React.FC<DropDownListProperties> = ({ icon, options, value, label, onChange }) => {
  return (
    <div className="flex flex-col w-full">
      <div className="flex flex-row w-max-full items-center gap-1.5">
        <Icon iconContent={IconContent[icon]} size={20} color={"#E8EEF1"} strokeWidth={2} />
        <select
          value={value}
          onChange={(e) => onChange(e)}
          className="outline-none text-main-text w-full text-lg bg-main-dark"
        >
          <option key="" value="">
            {label}
          </option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>
      <div className="w-full h-px bg-main-text mt-2"></div>
    </div>
  );
};

export default DropDownList;
