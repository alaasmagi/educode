import * as React from "react";
import { ColorValue } from "react-native";
import Svg, { Path } from "react-native-svg";

interface IconProperties {
  size: number;
  color: ColorValue;
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
  <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
    <Path
      d={iconContent}
      stroke={color}
      strokeWidth={strokeWidth}
      strokeLinecap={strokeLinecap}
      strokeLinejoin={strokeLinejoin}
    />
  </Svg>
);

export default Icon;
