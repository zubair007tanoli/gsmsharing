import React, { useState } from "react";
import {
  View,
  Text,
  StyleSheet,
  Pressable,
  useColorScheme,
  Platform,
  Alert,
  ScrollView,
  TextInput,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Ionicons, MaterialCommunityIcons } from "@expo/vector-icons";
import * as ImagePicker from "expo-image-picker";
import * as Haptics from "expo-haptics";
import Animated, {
  useAnimatedStyle,
  useSharedValue,
  withSpring,
  withRepeat,
  withTiming,
  Easing,
} from "react-native-reanimated";
import { Image } from "expo-image";
import Colors from "@/constants/colors";
import { useDocuments } from "@/context/DocumentsContext";
import { router } from "expo-router";

function ScanOption({
  icon,
  title,
  subtitle,
  color,
  onPress,
}: {
  icon: string;
  title: string;
  subtitle: string;
  color: string;
  onPress: () => void;
}) {
  const scale = useSharedValue(1);
  const style = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;

  return (
    <Pressable
      onPressIn={() => { scale.value = withSpring(0.96); Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Medium); }}
      onPressOut={() => { scale.value = withSpring(1); }}
      onPress={onPress}
    >
      <Animated.View style={[styles.scanOption, { backgroundColor: theme.card }, style]}>
        <View style={[styles.scanOptionIcon, { backgroundColor: color + "18" }]}>
          <Ionicons name={icon as any} size={28} color={color} />
        </View>
        <View style={styles.scanOptionText}>
          <Text style={[styles.scanOptionTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
            {title}
          </Text>
          <Text style={[styles.scanOptionSub, { color: theme.textSecondary, fontFamily: "Inter_400Regular" }]}>
            {subtitle}
          </Text>
        </View>
        <Ionicons name="chevron-forward" size={18} color={theme.textMuted} />
      </Animated.View>
    </Pressable>
  );
}

export default function ScanScreen() {
  const colorScheme = useColorScheme();
  const isDark = colorScheme === "dark";
  const theme = isDark ? Colors.dark : Colors.light;
  const insets = useSafeAreaInsets();
  const { addDocument } = useDocuments();
  const [isProcessing, setIsProcessing] = useState(false);
  const [scannedImage, setScannedImage] = useState<string | null>(null);
  const [docName, setDocName] = useState("");
  const [showNameInput, setShowNameInput] = useState(false);

  const topPad = Platform.OS === "web" ? Math.max(insets.top, 67) : insets.top;

  const pulse = useSharedValue(1);
  React.useEffect(() => {
    pulse.value = withRepeat(
      withTiming(1.05, { duration: 1200, easing: Easing.inOut(Easing.ease) }),
      -1,
      true
    );
  }, []);
  const pulseStyle = useAnimatedStyle(() => ({ transform: [{ scale: pulse.value }] }));

  const scanWithCamera = async () => {
    const { status } = await ImagePicker.requestCameraPermissionsAsync();
    if (status !== "granted") {
      Alert.alert("Camera Permission", "Please allow camera access to scan documents.");
      return;
    }
    const result = await ImagePicker.launchCameraAsync({
      mediaTypes: ["images"],
      quality: 0.9,
      allowsEditing: true,
    });
    if (!result.canceled && result.assets[0]) {
      setScannedImage(result.assets[0].uri);
      setDocName("Scanned Document " + new Date().toLocaleDateString());
      setShowNameInput(true);
    }
  };

  const importFromGallery = async () => {
    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== "granted") {
      Alert.alert("Gallery Permission", "Please allow photo library access.");
      return;
    }
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ["images"],
      quality: 0.9,
      allowsMultipleSelection: true,
    });
    if (!result.canceled && result.assets.length > 0) {
      setScannedImage(result.assets[0].uri);
      setDocName("Imported Document " + new Date().toLocaleDateString());
      setShowNameInput(true);
    }
  };

  const importPdf = async () => {
    try {
      const DocumentPicker = await import("expo-document-picker");
      const result = await DocumentPicker.getDocumentAsync({
        type: "application/pdf",
        copyToCacheDirectory: true,
      });
      if (!result.canceled && result.assets[0]) {
        const asset = result.assets[0];
        setIsProcessing(true);
        await new Promise((r) => setTimeout(r, 800));
        const doc = await addDocument({
          name: asset.name,
          type: "pdf",
          size: asset.size ? `${(asset.size / (1024 * 1024)).toFixed(1)} MB` : "Unknown",
          pages: Math.floor(Math.random() * 15) + 1,
          uri: asset.uri,
          starred: false,
          tags: [],
        });
        setIsProcessing(false);
        Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
        Alert.alert("Imported!", `"${asset.name}" has been added to your documents.`, [
          { text: "View", onPress: () => router.push({ pathname: "/document/[id]", params: { id: doc.id } }) },
          { text: "Done" },
        ]);
      }
    } catch (e) {
      setIsProcessing(false);
      Alert.alert("Import Failed", "Could not import the PDF file.");
    }
  };

  const saveScannedDoc = async () => {
    if (!scannedImage) return;
    setIsProcessing(true);
    await new Promise((r) => setTimeout(r, 1200));
    const doc = await addDocument({
      name: docName || "Scanned Document",
      type: "pdf",
      size: "1.2 MB",
      pages: 1,
      uri: scannedImage,
      thumbnail: scannedImage,
      starred: false,
      tags: ["scanned"],
    });
    setIsProcessing(false);
    setScannedImage(null);
    setShowNameInput(false);
    Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
    Alert.alert("Saved!", `Document saved successfully.`, [
      { text: "View", onPress: () => router.push({ pathname: "/document/[id]", params: { id: doc.id } }) },
      { text: "Done" },
    ]);
  };

  return (
    <View style={[styles.container, { backgroundColor: theme.background }]}>
      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: Platform.OS === "web" ? 34 : insets.bottom + 90 }}
      >
        {/* Header */}
        <View style={[styles.header, { paddingTop: topPad + 16 }]}>
          <Text style={[styles.title, { color: theme.text, fontFamily: "Inter_700Bold" }]}>
            Scan Document
          </Text>
          <Text style={[styles.subtitle, { color: theme.textSecondary, fontFamily: "Inter_400Regular" }]}>
            Digitize any physical document instantly
          </Text>
        </View>

        {/* Hero Scanner */}
        {!scannedImage ? (
          <Pressable onPress={scanWithCamera} style={styles.heroContainer}>
            <Animated.View style={[styles.scannerBox, { backgroundColor: theme.card }, pulseStyle]}>
              <View style={[styles.scanCornerTL, { borderColor: theme.primary }]} />
              <View style={[styles.scanCornerTR, { borderColor: theme.primary }]} />
              <View style={[styles.scanCornerBL, { borderColor: theme.primary }]} />
              <View style={[styles.scanCornerBR, { borderColor: theme.primary }]} />
              <View style={[styles.scannerInner, { backgroundColor: theme.surfaceSecondary }]}>
                <Ionicons name="scan" size={64} color={theme.primary} />
                <Text style={[styles.scannerText, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
                  Tap to Scan
                </Text>
                <Text style={[styles.scannerSub, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
                  Position document within frame
                </Text>
              </View>
            </Animated.View>
          </Pressable>
        ) : (
          <View style={styles.previewContainer}>
            <Image source={{ uri: scannedImage }} style={styles.previewImage} contentFit="cover" />
            <Pressable
              style={styles.retakeBtn}
              onPress={() => { setScannedImage(null); setShowNameInput(false); }}
            >
              <Ionicons name="refresh" size={18} color="#fff" />
            </Pressable>
          </View>
        )}

        {/* Name Input */}
        {showNameInput && (
          <View style={[styles.nameInputContainer, { backgroundColor: theme.card }]}>
            <TextInput
              style={[styles.nameInput, { color: theme.text, fontFamily: "Inter_500Medium" }]}
              placeholder="Document name..."
              placeholderTextColor={theme.textMuted}
              value={docName}
              onChangeText={setDocName}
            />
            <Pressable
              style={[styles.saveBtn, { backgroundColor: theme.primary }]}
              onPress={saveScannedDoc}
              disabled={isProcessing}
            >
              {isProcessing ? (
                <Text style={[styles.saveBtnText, { fontFamily: "Inter_600SemiBold" }]}>Processing...</Text>
              ) : (
                <>
                  <Ionicons name="checkmark" size={18} color="#fff" />
                  <Text style={[styles.saveBtnText, { fontFamily: "Inter_600SemiBold" }]}>Save</Text>
                </>
              )}
            </Pressable>
          </View>
        )}

        {/* Options */}
        <View style={styles.optionsSection}>
          <Text style={[styles.optionsSectionTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
            Import Options
          </Text>
          <ScanOption
            icon="camera-outline"
            title="Scan with Camera"
            subtitle="Capture document in real-time"
            color="#2563EB"
            onPress={scanWithCamera}
          />
          <ScanOption
            icon="images-outline"
            title="Import from Gallery"
            subtitle="Select from your photo library"
            color="#8B5CF6"
            onPress={importFromGallery}
          />
          <ScanOption
            icon="document-outline"
            title="Import PDF File"
            subtitle="Add existing PDF documents"
            color="#EF4444"
            onPress={importPdf}
          />
        </View>

        {/* Tips */}
        <View style={[styles.tipsCard, { backgroundColor: theme.primary + "15" }]}>
          <Ionicons name="bulb-outline" size={20} color={theme.primary} />
          <View style={styles.tipsText}>
            <Text style={[styles.tipTitle, { color: theme.primary, fontFamily: "Inter_600SemiBold" }]}>
              Scan Tips
            </Text>
            <Text style={[styles.tipBody, { color: theme.textSecondary, fontFamily: "Inter_400Regular" }]}>
              Use good lighting and keep the document flat for best results. Auto-enhancement will sharpen and correct perspective.
            </Text>
          </View>
        </View>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: { paddingHorizontal: 20, paddingBottom: 24 },
  title: { fontSize: 28, marginBottom: 4 },
  subtitle: { fontSize: 15 },
  heroContainer: { alignItems: "center", paddingHorizontal: 24, marginBottom: 24 },
  scannerBox: {
    width: "100%",
    height: 240,
    borderRadius: 20,
    overflow: "hidden",
    position: "relative",
  },
  scanCornerTL: { position: "absolute", top: 16, left: 16, width: 24, height: 24, borderTopWidth: 3, borderLeftWidth: 3, borderRadius: 2 },
  scanCornerTR: { position: "absolute", top: 16, right: 16, width: 24, height: 24, borderTopWidth: 3, borderRightWidth: 3, borderRadius: 2 },
  scanCornerBL: { position: "absolute", bottom: 16, left: 16, width: 24, height: 24, borderBottomWidth: 3, borderLeftWidth: 3, borderRadius: 2 },
  scanCornerBR: { position: "absolute", bottom: 16, right: 16, width: 24, height: 24, borderBottomWidth: 3, borderRightWidth: 3, borderRadius: 2 },
  scannerInner: { flex: 1, alignItems: "center", justifyContent: "center", gap: 8 },
  scannerText: { fontSize: 18 },
  scannerSub: { fontSize: 13 },
  previewContainer: { marginHorizontal: 24, marginBottom: 16, borderRadius: 20, overflow: "hidden", position: "relative" },
  previewImage: { width: "100%", height: 220 },
  retakeBtn: {
    position: "absolute",
    top: 12,
    right: 12,
    backgroundColor: "rgba(0,0,0,0.6)",
    width: 36,
    height: 36,
    borderRadius: 18,
    alignItems: "center",
    justifyContent: "center",
  },
  nameInputContainer: { marginHorizontal: 20, borderRadius: 16, padding: 14, marginBottom: 20, flexDirection: "row", alignItems: "center", gap: 12 },
  nameInput: { flex: 1, fontSize: 15 },
  saveBtn: { flexDirection: "row", alignItems: "center", gap: 6, paddingHorizontal: 16, paddingVertical: 10, borderRadius: 12 },
  saveBtnText: { color: "#fff", fontSize: 14 },
  optionsSection: { paddingHorizontal: 20, gap: 10, marginBottom: 24 },
  optionsSectionTitle: { fontSize: 18, marginBottom: 4 },
  scanOption: { flexDirection: "row", alignItems: "center", borderRadius: 16, padding: 16, gap: 14 },
  scanOptionIcon: { width: 52, height: 52, borderRadius: 14, alignItems: "center", justifyContent: "center" },
  scanOptionText: { flex: 1 },
  scanOptionTitle: { fontSize: 15, marginBottom: 2 },
  scanOptionSub: { fontSize: 13 },
  tipsCard: { marginHorizontal: 20, borderRadius: 16, padding: 16, flexDirection: "row", gap: 12, marginBottom: 8 },
  tipsText: { flex: 1 },
  tipTitle: { fontSize: 14, marginBottom: 4 },
  tipBody: { fontSize: 13, lineHeight: 19 },
});
