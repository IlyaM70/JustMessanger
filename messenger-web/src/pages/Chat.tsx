import React from 'react';
import { useState, useEffect, useRef } from 'react';

type ChatProps = {
    token: string;
    recipientId: string;
};

const Chat: React.FC<ChatProps> = ({ token, recipientId }) => {

    const [error, setError] = useState('');
    const [message, setMessage] = useState('');

    type Message = {
        text: string;
        isOwn: boolean;
    }

    const [messages, setMessages] = useState<Message[]>([]);
    const chatRef = useRef<HTMLDivElement>(null);

    // Auto-scroll to bottom when messages change
    useEffect(() => {
        if (chatRef.current) {
            chatRef.current.scrollTop = chatRef.current.scrollHeight;
        }
    }, [messages]);

    const AppendMessage = (text: string, isOwn: boolean) => {
        setMessages(prevMessages => [...prevMessages, { text, isOwn }]);
    }

    const Send = async () =>
    {
        try
        {
            const response = await fetch(`https://localhost:7136/api/Message/send`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    "Content-Type": "application/json" 
                },
                body: JSON.stringify({
                    recipientId: recipientId,
                    text: message,
                }),
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

        AppendMessage(message, true);
    }




    return (
        <div className="container-xl">
            <div className="row">
                <div className="col-2"></div>
                <div className="col-8">
                    {error && <div className="text-danger">{error}</div>}
                    <h1 className="text-center my-4">Recipient Name</h1>
                    <div id="chat" ref={chatRef} className="border rounded p-3 mb-3" style={{ height: '400px', overflowY: 'scroll' }}>
                        {/* Messages will be displayed here */}     
                        {messages.map((msg, index) => (
                            <div key={index} className={`mb-2 ${msg.isOwn ? 'text-start' : 'text-end'}`}>
                                <span className={`badge ${msg.isOwn ? 'bg-primary' : 'bg-secondary'}`}>{msg.text}</span>
                            </div>
                        )) }
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