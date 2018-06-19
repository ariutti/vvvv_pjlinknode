import socket, time

def closeSocketAndExit():
	print("Client: socket close")
	s.close()

	print( "Client: Press the 'Enter' key to exit...")
	m = input()
	exit()


# initial settings
ip_server = '127.0.0.1'
ip_port	  = 4352
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect( (ip_server, ip_port) )
print('Server: connected with ', ip_server, ' ', ip_port)

msg = s.recv(1024) # blocking
print( msg )
# the message should contain "PJ LINK 0"
if msg.decode() != 'PJ LINK 0':
	print( 'Client: connection not enstablished ' )
	closeSocketAndExit()

print()
print("Client: Send your PJ Link command then hit 'Enter'.")
print("\nSelect:\n\t'i' for INFO;\n\t'p' for POWR 1 (restart);\n\t'o' for POWR 0 (standby);\n\t'r' for SHDW R (reboot);\n\t's' for SHDW S (shutdown);")
print("Press 'spacebar' to exit!\n\n")
# main loop
while True:
	m = input() #blocking

	if m == 'i':
		msg = '%1INFO ?\r'
	elif m == 'p':
		msg = '%1POWR 1\r'
	elif m == 'o':
		msg = '%1POWR 0\r'
	elif m == 'r':
		msg = '%1SHDW R\r'
	elif m == 's':
		msg = '%1SHDW S\r'
	elif m == ' ':
		break
	else:
		print("Client: invalid character. valid ones are [i, p, o, r, 'spacebar']")
		continue

	print("Client: sending " + msg)
	s.sendall( msg.encode() )

	m = s.recv(1024) # blocking
	print( m )
	break

closeSocketAndExit()
