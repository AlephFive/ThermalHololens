import tornado.httpserver
import tornado.ioloop
import tornado.web
import tornado.websocket
import base64
import numpy as np
import cv2
from threading import Timer
import time

class WebSocketHandler(tornado.websocket.WebSocketHandler):
  
    def initialize(self):
        with open("output.png", "rb") as imageFile:
            self.str = base64.b64encode(imageFile.read())
      
    def open(self):
        print 'new connection'
        self.write_message(self.str)
        #while True:
        #    self.loop()
        #self.loop()
      
    def on_message(self, message):
        print 'new on_message'
        print message
        
        with open("output.png", "rb") as imageFile:
            self.str = base64.b64encode(imageFile.read())
        self.write_message(self.str)
      
    def on_close(self):
        print 'connection closed'

    def loop(self):
        with open("output.png", "rb") as imageFile:
            self.str = base64.b64encode(imageFile.read())
        self.write_message(self.str)
        time.sleep(1/20)
  
if __name__ == '__main__':

    application = tornado.web.Application([
    (r'/ws', WebSocketHandler),
    ])
    print "app start"
    http_server = tornado.httpserver.HTTPServer(application)
    http_server.listen(8008)
    tornado.ioloop.IOLoop.instance().start()
