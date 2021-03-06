import { Button, Popover } from "antd";
import Avatar from "antd/lib/avatar/avatar";
import { Header } from "antd/lib/layout/layout";
import axios from "axios";
import { useEffect } from "react";
import { useState } from "react/cjs/react.development";
import { Logout } from "../Logout/logout.component";
import Cookies from "js-cookie";
import jwt_decode from "jwt-decode";
import "./admin-page.styles.css"
import ManageWorkforce from "./manage-workforce/manage-workforce.component";

const AdminPage = () => {
    const [userdata, setUserdata] = useState("");
    const [showManage, setShowManage] = useState(true);
    

    useEffect(() => {
        const token = Cookies.get("auth");
        const decoded = jwt_decode(token);
        const username = decoded.sub;
        setUserdata(username);
    })

    return (
        <div className="admin-page">
            <Header style={{ zIndex: 1, width: "100%" }}>
                <div className="logoLeftSide" style={{ width: "200px" }}>
                    <div className="logo" style={{ marginTop: "7px" }}>
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="50"
                            height="50"
                            viewBox="0 0 949 945"
                        >
                            <g>
                                <g>
                                    <path
                                        fill="#00dcc7"
                                        d="M619.626 23.108c-46.106 93.893-85.39 215.437-143.502 331.467-15.963-12.454-98.82-266.173-128.657-257.258-23.051-3.285-26.392 3.298-34.639 4.947-68.712 80.32-64.767 165.325-118.76 252.31-2.438-1.931-.893-.618-6.682-10.361-10.908-30.945-23.487-56.02-77.44-172.687C294.735-34.631 501.342-10.106 619.626 23.108zm79.173 34.63c287.213 134.043 289.748 530.096 192.986 628.304-100.4-231.39-91.273-265.292-158.347-242.416-19.439 19.375-25.64 20.099-59.38 113.787-69.923 193.506-42.454 174.232-148.45-79.157 45.977-111.163 113.817-278.71 173.191-420.517zM332.622 250.683c17.111 15.153 55.651 107.645 98.967 232.522-48.009 81.184-106.377 263.362-183.09 405.675C74.51 815.297-89.048 508.264 55.515 260.577c75.118 116.43 64.27 240.368 138.554 252.31 66.503 6.05 112.908-225.535 138.554-262.205zm138.553 341.362c60.486 97.757 55.872 129.174 108.864 237.468 8.029 15.645 40.658 38.833 74.225 4.948 77.744-176.827 80.447-192.805 103.915-242.416 45.49 114.962 55.313 138.11 79.174 192.943-35.455 40.362-175.67 213.585-499.783 143.47-8.213-20.844 132.595-285.673 133.605-336.413z"
                                    />
                                </g>
                            </g>
                        </svg>
                    </div>
                    <span style={{ marginLeft: "20px" }}>Wman</span>
                </div>
                <div className="select-intervall">
                    <div>
                        <Button onClick={() => setShowManage(true)} >Manage Workforce</Button>
                    </div>
                    <div>
                        <Button onClick={() => setShowManage(false)} >Display workloads</Button>
                    </div>
                </div>

                <Popover placement="bottomRight"
                    className="worker-avatar-popover"
                    content=
                    {<><div className="popover-name">
                        {userdata}
                    </div>
                        <div className="logout">
                            <Logout />
                        </div></>}>
                    <Avatar
                        src={`https://eu.ui-avatars.com/api?name=${userdata}`} />
                </Popover>
            </Header>
            {showManage ? 
                <ManageWorkforce/> :null}

        </div>
    )
}
export default AdminPage;