interface DateSelectorProperties {
  value: string;
  onChange: (event: React.ChangeEvent<HTMLInputElement>) => void;
}

const DateSelector: React.FC<DateSelectorProperties> = ({
  value,
  onChange,
}) => {
  return (
    <label className="cursor-pointer">
      <input
        type="date"
        value={value}
        onChange={onChange}
        className="text-main-text text-xl"
      />
    </label>
  );
};

export default DateSelector;
