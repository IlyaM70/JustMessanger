import React from 'react';
import { useState } from 'react';

type ChatProps = {
    token: string;
    recipientId: string;
};

const Chat: React.FC<ChatProps> = ({ token, recipientId }) => {

    const [error, setError] = useState('');
    const [message, setMessage] = useState('');

    const Send = async () =>
    {
        try
        {
            const response = await fetch(`https://localhost:7136/api/Message/send?recipientId=${encodeURIComponent(recipientId)}&text=${encodeURIComponent(message)}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                },
            });

            const data = await response.json();

            if (!response.ok) {

                if (data.errors) {
                    setError(Object.values(data.errors).flat().join(' '));
                    return;
                }

                setError('Network error');
                return;
            }


        }
        catch (error)
        {
            setError('Network error');
        }
    }




    return (
        <div className="container-xl">
            <div className="row">
                <div className="col-2"></div>
                <div className="col-8">
                    {error && <div className="text-danger">{error}</div>}
                    <h1 className="text-center my-4">Recipient Name</h1>
                    <div className="border rounded p-3 mb-3" style={{ height: '400px', overflowY: 'scroll' }}>
                        {/* Messages will be displayed here */}     
                        <div className="mb-2">
                            <strong>User 1:</strong> Hello!
                        </div>
                        <div className="mb-2 text-end">
                            <strong>You:</strong> Hi there!
                        </div>
                        <div className="mb-2">
                            <strong>User 1:</strong> How are you?
                        </div>
                        <div className="mb-2 text-end">
                            <strong>You:</strong> I'm good, thanks! And you?
                        </div>
                    </div>
                    <div className="input-group mb-3">
                        <input value={message} onChange={(e) => setMessage(e.target.value)} type="text" className="form-control" placeholder="Type your message..." />
                        <button onClick={Send} className="btn btn-primary" type="button">Send</button>
                    </div>
                </div>
                <div className="col-2"></div>
            </div>

    </div>);
}

export default Chat;