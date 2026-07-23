import "./button.style.css";
interface ButtonProps {
  style: "primary" | "secondary";
  onClickButton: (value: boolean) => void;
  content: string;
}

function Button({ style = "primary", content, onClickButton }: ButtonProps) {
  return (
    <button
      className={
        style == "primary"
          ? "buttomPrimary"
          : style == "secondary"
            ? "buttomSecondary"
            : "buttomPrimary"
      }
      onClick={() => {
        onClickButton(true);
      }}
    >
      {content}
    </button>
  );
}
export default Button;
