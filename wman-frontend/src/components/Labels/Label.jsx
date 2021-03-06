import React from "react";
import { Tag } from "antd";

export default function Label(props) {
  return (
    <Tag
      className="labelOnCard"
      color={props.backgroundColor}
      onMouseDown={props.onPreventMouseDown}
      closable={props.closable}
      onClose={props.onClose}
    >
      {props.name}
    </Tag>
  );
}
