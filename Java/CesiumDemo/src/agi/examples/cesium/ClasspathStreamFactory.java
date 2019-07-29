package agi.examples.cesium;

import java.io.IOException;
import java.io.InputStream;
import java.io.UncheckedIOException;

import agi.foundation.compatibility.MemoryStream;
import agi.foundation.infrastructure.StreamFactory;

/**
 * Provides StreamFactory support for reading a resource from the classpath. 
 */
public class ClasspathStreamFactory extends StreamFactory {
    private final String m_classpathResourceName;

    public ClasspathStreamFactory(String classpathResourceName) {
        m_classpathResourceName = classpathResourceName;
    }

    @Override
    public InputStream openStream() {
        // StreamFactory streams must be seekable, so we copy bytes to a MemoryStream.
        MemoryStream memoryStream = new MemoryStream();
        try (InputStream stream = ClasspathStreamFactory.class.getResourceAsStream(m_classpathResourceName)) {
            byte[] buffer = new byte[8 * 1024];

            int count;
            while ((count = stream.read(buffer, 0, buffer.length)) > 0) {
                memoryStream.write(buffer, 0, count);
            }
            memoryStream.setPosition(0L);
        } catch (IOException e) {
            throw new UncheckedIOException(e);
        }

        return memoryStream;
    }

}
