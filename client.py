import socket, time

def closeSocketAndExit():
	print("Client: socket close")
	s.close()

	print( "Client: Press the 'Enter' key to exit...")
	m = input()
	exit()


# initial settings
ip_server = '127.0.0.1'
#ip_server = '192.168.1.66'
ip_port	  = 4352
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect( (ip_server, ip_port) )

'''
try:
	s.connect( (ip_server, ip_port) )
exec:
	print("error in creating the connection")
'''

print('Server: connected with ', ip_server, ' ', ip_port)

'''
# expecting PJ LINK 0 is no more needed
msg = s.recv(1024) # blocking
print( msg )
# the message should contain "PJ LINK 0"
if msg.decode() != 'PJ LINK 0':
	print( 'Client: connection not enstablished.' )
	closeSocketAndExit()

print()
print( 'Client: connection enstablished! ("PJ LINK 0" received)' )
'''

print( 'Client: connection enstablished!')
print("Send your PJ Link command:\n")
print("\t'g' for POWR ? (get power status);\n\t'i' for INFO ? (get information);\n\t'p' for POWR 1 (restart);\n\t'o' for POWR 0 (standby);\n\t'r' for SHDW R (reboot);\n\t's' for SHDW S (shutdown);")
print("...then hit 'Enter'. Press 'spacebar' to exit!\n\n")

# main loop
while True:

	m = input() #blocking

	if m == 'g':
		msg = '%1POWR ?\r'
	elif m == 'i':
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
		print("Client: invalid character. valid ones are [g, i, p, o, r, 'spacebar']")
		continue

	print("Client: sending " + msg)
	s.sendall( msg.encode() )

	m = s.recv(1024) # blocking
	print( m )
	break

closeSocketAndExit()
# NOTE: solve an issue with the socket not closed
