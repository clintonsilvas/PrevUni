import useState from "react";
import "./inputText.style.css";

interface InputTextProps {
  name: string;
  placeholder?: string;
  showLabel?: boolean;
  typeInput?: "text" | "email" | "tel" | "password";
  labelText?: string;
  onAddContent: (value: string) => void;
}

function InputText({
  name,
  placeholder = "Insira Aqui...",
  showLabel = true,
  labelText = "Campo",
  typeInput = "text",
  onAddContent,
}: InputTextProps) {
  return (
    <div className="input-container-single">
      {showLabel ? <label htmlFor={name}>{labelText}</label> : null}

      <input
        type={typeInput}
        id={name}
        name={name}
        placeholder={placeholder}
        onChange={(e) => {
          onAddContent(e.target.value);
        }}
      ></input>
    </div>
  );
}
export default InputText;
