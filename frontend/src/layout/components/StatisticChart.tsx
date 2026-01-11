import React from "react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
  ResponsiveContainer,
} from "recharts";
import StudentCountModel from "../../models/StudentCountModel";

interface StatisticChartProperties {
  data: StudentCountModel[];
}

const StatisticChart: React.FC<StatisticChartProperties> = ({ data }) => {
  return (
    <ResponsiveContainer width="100%" height={500}>
      <BarChart
        data={data}
        margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
      >
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="attendanceDate" />
        <YAxis />
        <Tooltip />
        <Bar dataKey="studentCount" fill="#4492EA" barSize={50} />
      </BarChart>
    </ResponsiveContainer>
  );
};

export default StatisticChart;
