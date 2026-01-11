import * as React from "react";

interface IconProperties {
  size: number;
  color: string;
  iconContent: string;
  strokeWidth: number;
  strokeLinecap?: "round" | "butt" | "square";
  strokeLinejoin?: "round" | "miter" | "bevel";
}

const Icon: React.FC<IconProperties> = ({
  size,
  color,
  iconContent,
  strokeWidth,
  strokeLinecap = "round",
  strokeLinejoin = "round",
}) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none">
    <path
      d={iconContent}
      stroke={color}
      strokeWidth={strokeWidth}
      strokeLinecap={strokeLinecap}
      strokeLinejoin={strokeLinejoin}
    />
  </svg>
);

export default Icon;
