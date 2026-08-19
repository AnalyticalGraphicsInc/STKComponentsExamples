package agi.examples;

import java.io.IOException;
import java.io.InputStream;
import java.io.UncheckedIOException;
import java.net.URL;

import agi.foundation.compatibility.ArgumentException;
import agi.foundation.compatibility.MemoryStream;
import agi.foundation.infrastructure.StreamFactory;

/**
 * Provides StreamFactory support for reading a resource from the classpath. 
 */
public class ClasspathStreamFactory extends StreamFactory {
    private static final int bufferSize = 8 * 1024;
    private final URL resourceURL;
    private final boolean seekable;

    public ClasspathStreamFactory(String classpathResourceName, boolean seekable) {
        this.resourceURL = locateResource(classpathResourceName);
        if (this.resourceURL == null)
            throw new ArgumentException("Resource not found on classpath: " + classpathResourceName);
        this.seekable = seekable;
    }

    public ClasspathStreamFactory(URL classpathResourceURL, boolean seekable) {
        this.resourceURL = classpathResourceURL;
        this.seekable = seekable;
    }

    public static URL locateResource(String classpathResourceName) {
        return ClasspathStreamFactory.class.getClassLoader().getResource(classpathResourceName);
    }

    @Override
    public InputStream openStream() {
        try (InputStream stream = resourceURL.openStream()) {
            if (seekable) {
                // to make stream seekable, copy bytes to a MemoryStream.
                MemoryStream memoryStream = new MemoryStream();
                byte[] buffer = new byte[bufferSize];

                int count;
                while ((count = stream.read(buffer, 0, buffer.length)) > 0) {
                    memoryStream.write(buffer, 0, count);
                }
                memoryStream.setPosition(0L);

                return memoryStream;
            }

            return stream;
        } catch (IOException e) {
            throw new UncheckedIOException(e);
        }
    }
}
