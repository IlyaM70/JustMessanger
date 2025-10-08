import { useEffect, useState } from "react";
import { jwtDecode } from 'jwt-decode';
import { useNavigate } from "react-router-dom";

type Contact = {
  userId: string;
  email: string;
  userName: string;
  lastMessage: string;
  lastMessageAt?: string;
};

interface TokenPayload {
    uid: string;
    email: string;
    expiration: number;
}

const ContactList: React.FC = () => {

  const [contacts, setContacts] = useState<Contact[]>([]);
  const [currentUserId, setCurrentUserId] = useState<string>('');
  const token = localStorage.getItem('token') || '';
  const navigate = useNavigate();
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

  
  // Fetch contacts from an API
  useEffect(() => {
  if (!currentUserId) return; // Don't call API if userId isn't ready

  const fetchContacts = async () => {
    try {
      const response = await fetch(
        `https://localhost:7136/api/Message/contacts?userId=${currentUserId}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      if (!response.ok) {
        throw new Error("Failed to fetch contacts");
      }

      const data = await response.json();
      setContacts(data.value);
    } catch (err) {
      console.error("Error fetching contacts:", err);
    }
  };

  fetchContacts();
}, [currentUserId, token]);

  const openChat = (recipientId: string, recipientName: string) => {
    navigate('/chat', { state: { recipientId, recipientName } });
  }

  const newChat = () => {
    navigate('/chat');
  }

  return (
    <>
    <div className="d-flex">
      <button className="flex-start" onClick={newChat}>New Chat</button>
    </div>
     <div className="list-group shadow-sm rounded">
      {contacts.length === 0 && (
        <div className="text-center text-muted py-3">No contacts yet</div>
      )}

      {contacts.map((c) => (        
        <button
          onClick={() => openChat(c.userId, c.userName)}
          key={c.userId}
          className={`list-group-item list-group-item-action d-flex justify-content-between align-items-center`}
        >
          <div>            
            <div className="fw-bold">{c.userName}</div>
            <div className="text-muted small text-truncate" style={{ maxWidth: "200px" }}>
              {c.lastMessage}
            </div>
          </div>
          {c.lastMessageAt && (
            <small className="text-muted">
              {new Date(c.lastMessageAt).toLocaleTimeString([], { day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit" })}
            </small>
          )}
        </button>
      ))}
    </div>
    </>
   
  );
};

export default ContactList;
