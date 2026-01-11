import React from "react";
import { IconContent} from "./Icons";
import Icon from "./Icon";

interface DataTextProperties {
    icon: keyof typeof IconContent;
    text: string;
}

const DataText: React.FC<DataTextProperties> = ({ icon, text }) => {
  return (
    <div className="flex flex-row gap-3 items-center">
        <Icon iconContent={IconContent[icon]} size={20} color={"#E8EEF1"} strokeWidth={2} />
        <span className="text-xl">{text}</span>
    </div>
  );
};

export default DataText;