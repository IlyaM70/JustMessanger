import React from 'react';
import { useState, useEffect, useRef } from 'react';
import { HubConnectionBuilder } from "@microsoft/signalr";
import { jwtDecode } from 'jwt-decode';
import { useLocation } from 'react-router-dom';

type Message = {
    text: string;
    isOwn: boolean;
}
interface TokenPayload {
    uid: string;
    email: string;
    expiration: number;
}

interface MessageData {
    id: string;
    recipientId: string;
    senderId: string;
    sentAt: string;
    text: string;
}


const Chat: React.FC = () => {

    const authApiBase = 'https://just-messenger-auth.azurewebsites.net/';
    const authApiUrl = `${authApiBase}/api/Auth`;
    const messageApiBase = 'https://just-messenger-messages.azurewebsites.net/';
    const messageApiUrl = `${messageApiBase}/api/Message`;
    const location = useLocation();
    const [recipientId, setRecipientId] = useState<string>(location.state?.recipientId || '');
    const [recipientName, setRecipientName] = useState<string>(location.state?.recipientName || 'New Chat');
    const [currentUserId, setCurrentUserId] = useState<string>('');
    const [error, setError] = useState('');
    const [message, setMessage] = useState('');
    const [messages, setMessages] = useState<Message[]>([]);
    const token = localStorage.getItem('token') || '';
    const [emailInput, setEmailInput] = useState('');

    //get user id from token
    useEffect(() => {
        try
        {
            const decoded = jwtDecode<TokenPayload>(token);            
            setCurrentUserId(decoded.uid);
        }
        catch (error)
        {
            console.error("Invalid token:", error);
        }
    }, [token]);

    const parseMessages = (data: MessageData[], currentUserId: string): Message[] => {
        return data.map(msg => ({
            text: msg.text,
            isOwn: msg.senderId === currentUserId, // true if you sent it
        }));
    };

    //get message history  
    useEffect(() => {
        if (!currentUserId || !recipientId) return; // Don't call API if userId or recipientId isn't ready
        const fetchHistory = async () => {
            try {
                const response = await fetch(
                    `${messageApiUrl}/history?userId=${currentUserId}&otherUserId=${recipientId}`,
                    {
                        headers: {
                            Authorization: `Bearer ${token}`,
                        },
                    }
                );

                if (!response.ok) {
                    throw new Error("Failed to fetch history");
                }

                const data = await response.json();
                const parsed: Message[] = parseMessages(data, currentUserId);
                setMessages(parsed);
            } catch (err) {
                console.error("Error fetching messages:", err);
            }
        };

        fetchHistory();
    }, [currentUserId, recipientId, token]);

    //set up web socket connection
    useEffect(() => {
        if (!currentUserId || !recipientId) return; // Don't call API if userId or recipientId isn't ready
        const connection = new HubConnectionBuilder()
            .withUrl(`${messageApiBase}/messagesHub?userId=${currentUserId}`, {
                accessTokenFactory: () => token,
            })
            .withAutomaticReconnect()
            .build();

        connection.start().then(() => {
            //console.log("Connected to SignalR hub");

            connection.on("ReceiveMessage", (msg) => {
                setMessages((prevMessages) => [...prevMessages, msg]);
            });
        });

        return () => {
            connection.stop();
        };
    }, [token, currentUserId]);

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
            const response = await fetch(`${messageApiUrl}/send`, {
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
            
            if (!response.ok) {
                
                const data = await response.json();
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
            setError('Network error: ' + error);
        }

        AppendMessage(message, true);
    }

    const getRecipientByEmail = async () => {
        try {
            const response = await fetch(`${authApiUrl}/getContactByEmail?email=${emailInput}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                },
            });

            if (!response.ok) {
                setError('Failed to fetch recipient');
                return;
            }

            const data = await response.json();
            setRecipientId(data.userId);
            setRecipientName(data.userName);
        } catch (error) {
            setError('Error fetching recipient: ' + error);
        }
    }

    return (
        <div className="page-container">
            <div className="chat-layout">
                {/* Contact column - for future */}
                <div></div>

                <div className="chat-column">
                    <div className="chat-panel">
                        <div className="panel-header">
                            <div style={{display:'flex', gap:12, alignItems:'center'}}>
                                <div style={{width:44, height:44, borderRadius:10, background:'#eef3ff', display:'flex', alignItems:'center', justifyContent:'center', fontWeight:700}}>
                                    {recipientName?.slice(0,2).toUpperCase()}
                                </div>
                                <div>
                                    <div className="title">{recipientName}</div>
                                    <div className="sub small-muted">last seen recently</div>
                                </div>
                            </div>
                        </div>

                        <div className="messages-area" id="chat" ref={chatRef} style={{minHeight: 260}}>
                            {error && <div className="text-danger">{error}</div>}
                            {/* If new chat enter recipient email */}
                            {!recipientId && 
                                <div className="center-placeholder">
                                    <div style={{maxWidth:420}}>
                                        <h5>Start a new chat</h5>
                                        <p className="small-muted">Find recipient by email</p>
                                        <div className="d-flex gap-2">
                                            <input value={emailInput} onChange={(e) => setEmailInput(e.target.value)} className="form-control input-rounded" placeholder="Recipient email..." />
                                            <button onClick={getRecipientByEmail} className="btn btn-pill btn-outline-primary" type="button">Search</button>
                                        </div>
                                    </div>
                                </div>
                            }

                            {/* Messages */}
                            {messages.map((msg, index) => (
                                <div key={index} className={`msg-row ${msg.isOwn ? 'own' : ''}`}>
                                    {!msg.isOwn && <div className="avatar-sm">{recipientName?.slice(0,1).toUpperCase()}</div>}
                                    <div className={`msg-bubble ${msg.isOwn ? 'right' : 'left'}`}>
                                        <div>{msg.text}</div>
                                        <div className={msg.isOwn ? 'msg-time' : 'msg-time-left'}>{/* you can add time if message object has it */}</div>
                                    </div>
                                    {msg.isOwn && <div style={{width:36}}></div>}
                                </div>
                            ))}
                        </div>

                        <div className="chat-input">
                            <div className="input-flex">
                                <input disabled={!recipientId} value={message} onChange={(e) => setMessage(e.target.value)} onKeyDown={(e)=>{ if (e.key === 'Enter') Send(); }} type="text" className="form-control input-rounded" placeholder={recipientId ? "Type your message..." : "Select or search a recipient"} />
                            </div>
                            <div>
                                <button disabled={!recipientId} onClick={Send} className="btn btn-send" type="button">Send</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default Chat;
