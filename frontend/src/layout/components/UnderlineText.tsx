import React from "react";

interface UnderlinetextProperties {
  text: string;
}
const UnderlineText: React.FC<UnderlinetextProperties> = ({ text }) => {
  return <span className="text-2xl text-main-text underline max-w-85">{text}</span>;
};

export default UnderlineText;
