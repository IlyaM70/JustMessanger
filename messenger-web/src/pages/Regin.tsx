import React from 'react';
import { useState } from 'react';


type LoginProps = {
    isRegister: boolean;
};


const Regin: React.FC<LoginProps> = ({ isRegister}) => {

    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');  
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    const loginHandler = async () => {
        setError('');
        setSuccess('');

        try
        {
            const response = await fetch('http://localhost:5027/api/Auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });

            console.log(response);
            const data = await response.json();
            console.log(data);

            if (!response.ok) {

                if (data.errors)
                {
                    setError(Object.values(data.errors).flat().join(' '));
                    return;
                }

                setError('Login failed');
                return;
            }

            localStorage.setItem('token', data.token);
            setSuccess('Login successful!');
        }
        catch (error)
        {
            setError('Network error');
        }
    };

    const registerHandler = async () => {
        setError('');
        setSuccess('');

        try {
            const response = await fetch('http://localhost:5027/api/Auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, email, password })
            });

            console.log(response);
            const data = await response.json();
            console.log(data);

            if (!response.ok) {

                if (data.errors) {
                    setError(Object.values(data.errors).flat().join(' '));
                    return;
                }

                setError('Register failed');
                return;
            }
            
            setSuccess('Register successful!');
        }
        catch (error) {
            setError('Network error');
        }
    };



    return (
        <div className="container-xl">
            <div className="row">
                <div className="col-3"></div>
                <div className="col-6">
                    <h1 className="text-center my-3">{isRegister? "Register":"Login"}</h1>
                    {isRegister &&
                        <input type="text"
                        className="form-control my-2"
                        placeholder="Username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                    />}
                    <input type="text"
                        className="form-control my-2"
                        placeholder="Email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                    <input type="password"
                        className="form-control my-2"
                        placeholder="Password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                    {isRegister &&
                        <button className="btn btn-primary w-100 my-2" onClick={registerHandler}>Register</button>}
                    {!isRegister &&
                    <button className="btn btn-primary w-100 my-2" onClick={loginHandler}>Login</button>}
                    {error && <div className="text-danger">{error}</div>}
                    {success && <div className="text-success">{success}</div>}

                </div>
                <div className="col-3"></div>
            </div>
        </div>
    );

};




export default Regin;
