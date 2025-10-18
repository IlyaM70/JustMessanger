import React from 'react';
import { useState } from 'react';


type LoginProps = {
    isRegister: boolean;
};


const Regin: React.FC<LoginProps> = ({ isRegister}) => {

    const authApiUrl = 'http://localhost:5027/api/Auth';
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
            const response = await fetch(`${authApiUrl}/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });

            
            const data = await response.json();
            

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
            const response = await fetch(`${authApiUrl}/register`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, email, password })
            });

            //console.log(response);
            const data = await response.json();
            //console.log(data);

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
        <div className="page-container">
            <div className="row justify-content-center">
                <div className="col-lg-5 col-md-7">
                    <div className="card" style={{borderRadius:12, boxShadow:'0 10px 30px rgba(33,41,66,0.06)'}}>
                        <div className="p-4">
                            <h3 className="text-center mb-2">{isRegister? "Create account":"Welcome back"}</h3>
                            <p className="text-center small-muted mb-3">{isRegister ? 'Register to start chatting' : 'Sign in to continue'}</p>

                            {isRegister &&
                                <input type="text"
                                className="form-control my-2 input-rounded"
                                placeholder="Username"
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                            />}

                            <input type="text"
                                className="form-control my-2 input-rounded"
                                placeholder="Email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                            />
                            <input type="password"
                                className="form-control my-2 input-rounded"
                                placeholder="Password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                            />
                            {isRegister &&
                                <button className="btn btn-primary w-100 my-2 btn-send" onClick={registerHandler}>Register</button>}
                            {!isRegister &&
                            <button className="btn btn-primary w-100 my-2 btn-send" onClick={loginHandler}>Login</button>}

                            {error && <div className="text-danger small mt-2">{error}</div>}
                            {success && <div className="text-success small mt-2">{success}</div>}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );

};

export default Regin;
