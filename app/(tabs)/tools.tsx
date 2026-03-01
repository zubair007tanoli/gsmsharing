import React, { useState } from "react";
import {
  View,
  Text,
  StyleSheet,
  Pressable,
  useColorScheme,
  Platform,
  ScrollView,
  Alert,
  Modal,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Ionicons, MaterialCommunityIcons } from "@expo/vector-icons";
import * as Haptics from "expo-haptics";
import Animated, { useAnimatedStyle, useSharedValue, withSpring } from "react-native-reanimated";
import Colors from "@/constants/colors";

interface Tool {
  id: string;
  name: string;
  description: string;
  icon: string;
  iconLib: "ionicons" | "mci";
  color: string;
  category: string;
  badge?: string;
}

const PDF_TOOLS: Tool[] = [
  { id: "merge", name: "Merge PDF", description: "Combine multiple PDFs into one", icon: "call-merge", iconLib: "mci", color: "#2563EB", category: "Organize" },
  { id: "split", name: "Split PDF", description: "Extract pages into separate files", icon: "call-split", iconLib: "mci", color: "#8B5CF6", category: "Organize" },
  { id: "compress", name: "Compress PDF", description: "Reduce file size without quality loss", icon: "compress", iconLib: "mci", color: "#10B981", category: "Optimize", badge: "Popular" },
  { id: "rotate", name: "Rotate Pages", description: "Rotate individual or all pages", icon: "rotate-right", iconLib: "mci", color: "#F59E0B", category: "Organize" },
  { id: "reorder", name: "Reorder Pages", description: "Drag and drop to rearrange pages", icon: "sort", iconLib: "mci", color: "#EC4899", category: "Organize" },
  { id: "delete-pages", name: "Delete Pages", description: "Remove unwanted pages from PDF", icon: "delete", iconLib: "mci", color: "#EF4444", category: "Organize" },
  { id: "pdf-to-word", name: "PDF to Word", description: "Convert PDF to editable Word doc", icon: "file-word", iconLib: "mci", color: "#2563EB", category: "Convert", badge: "New" },
  { id: "pdf-to-excel", name: "PDF to Excel", description: "Extract tables into spreadsheet", icon: "file-excel", iconLib: "mci", color: "#10B981", category: "Convert" },
  { id: "pdf-to-jpg", name: "PDF to Image", description: "Export pages as JPEG or PNG", icon: "file-image", iconLib: "mci", color: "#8B5CF6", category: "Convert" },
  { id: "word-to-pdf", name: "Word to PDF", description: "Convert Word documents to PDF", icon: "file-pdf-box", iconLib: "mci", color: "#EF4444", category: "Convert" },
  { id: "jpg-to-pdf", name: "Image to PDF", description: "Turn photos into a PDF document", icon: "image-multiple", iconLib: "mci", color: "#F59E0B", category: "Convert", badge: "Popular" },
  { id: "ocr", name: "OCR Text", description: "Extract text from scanned documents", icon: "text-recognition", iconLib: "mci", color: "#06B6D4", category: "AI Tools", badge: "AI" },
  { id: "sign", name: "Sign PDF", description: "Add your signature to documents", icon: "draw", iconLib: "mci", color: "#8B5CF6", category: "Edit" },
  { id: "watermark", name: "Watermark", description: "Add text or image watermark", icon: "watermark", iconLib: "mci", color: "#F59E0B", category: "Edit" },
  { id: "password", name: "Protect PDF", description: "Add password encryption", icon: "lock", iconLib: "mci", color: "#EF4444", category: "Security" },
  { id: "unlock", name: "Unlock PDF", description: "Remove PDF password protection", icon: "lock-open", iconLib: "mci", color: "#10B981", category: "Security" },
  { id: "redact", name: "Redact Text", description: "Permanently hide sensitive info", icon: "eraser", iconLib: "mci", color: "#64748B", category: "Security" },
  { id: "fill-form", name: "Fill Forms", description: "Complete PDF forms digitally", icon: "form-textbox", iconLib: "mci", color: "#2563EB", category: "Edit" },
  { id: "annotate", name: "Annotate", description: "Add notes, highlights and drawings", icon: "pencil", iconLib: "mci", color: "#EC4899", category: "Edit" },
  { id: "summarize", name: "AI Summarize", description: "Get key insights from any PDF", icon: "robot", iconLib: "mci", color: "#06B6D4", category: "AI Tools", badge: "AI" },
];

const CATEGORIES = ["All", "Organize", "Convert", "Edit", "Security", "AI Tools", "Optimize"];

function ToolIcon({ tool, size = 22 }: { tool: Tool; size?: number }) {
  return <MaterialCommunityIcons name={tool.icon as any} size={size} color={tool.color} />;
}

function AnimatedToolCard({ tool, onPress }: { tool: Tool; onPress: () => void }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  const scale = useSharedValue(1);
  const style = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));

  return (
    <Pressable
      onPressIn={() => { scale.value = withSpring(0.95); Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light); }}
      onPressOut={() => { scale.value = withSpring(1); }}
      onPress={onPress}
      style={{ flex: 1 }}
    >
      <Animated.View style={[styles.toolCard, { backgroundColor: theme.card }, style]}>
        <View style={styles.toolCardHeader}>
          <View style={[styles.toolCardIcon, { backgroundColor: tool.color + "18" }]}>
            <ToolIcon tool={tool} size={24} />
          </View>
          {tool.badge && (
            <View style={[styles.badge, {
              backgroundColor: tool.badge === "AI" ? "#06B6D4" : tool.badge === "New" ? "#8B5CF6" : "#2563EB"
            }]}>
              <Text style={[styles.badgeText, { fontFamily: "Inter_600SemiBold" }]}>{tool.badge}</Text>
            </View>
          )}
        </View>
        <Text style={[styles.toolCardName, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
          {tool.name}
        </Text>
        <Text style={[styles.toolCardDesc, { color: theme.textSecondary, fontFamily: "Inter_400Regular" }]} numberOfLines={2}>
          {tool.description}
        </Text>
      </Animated.View>
    </Pressable>
  );
}

export default function ToolsScreen() {
  const colorScheme = useColorScheme();
  const isDark = colorScheme === "dark";
  const theme = isDark ? Colors.dark : Colors.light;
  const insets = useSafeAreaInsets();
  const [selectedCategory, setSelectedCategory] = useState("All");
  const [selectedTool, setSelectedTool] = useState<Tool | null>(null);

  const topPad = Platform.OS === "web" ? Math.max(insets.top, 67) : insets.top;

  const filteredTools = selectedCategory === "All"
    ? PDF_TOOLS
    : PDF_TOOLS.filter((t) => t.category === selectedCategory);

  const handleToolPress = (tool: Tool) => {
    setSelectedTool(tool);
  };

  const rows: Tool[][] = [];
  for (let i = 0; i < filteredTools.length; i += 2) {
    rows.push(filteredTools.slice(i, i + 2));
  }

  return (
    <View style={[styles.container, { backgroundColor: theme.background }]}>
      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: Platform.OS === "web" ? 34 : insets.bottom + 90 }}
      >
        {/* Header */}
        <View style={[styles.header, { paddingTop: topPad + 16 }]}>
          <Text style={[styles.title, { color: theme.text, fontFamily: "Inter_700Bold" }]}>PDF Tools</Text>
          <Text style={[styles.subtitle, { color: theme.textSecondary, fontFamily: "Inter_400Regular" }]}>
            {PDF_TOOLS.length} tools to power your workflow
          </Text>
        </View>

        {/* Categories */}
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.categories}
        >
          {CATEGORIES.map((cat) => (
            <Pressable
              key={cat}
              style={[
                styles.categoryChip,
                { backgroundColor: selectedCategory === cat ? theme.primary : theme.card }
              ]}
              onPress={() => { setSelectedCategory(cat); Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light); }}
            >
              <Text style={[
                styles.categoryText,
                { color: selectedCategory === cat ? "#fff" : theme.textSecondary, fontFamily: "Inter_500Medium" }
              ]}>
                {cat}
              </Text>
            </Pressable>
          ))}
        </ScrollView>

        {/* Tools Grid */}
        <View style={styles.toolsGrid}>
          {rows.map((row, i) => (
            <View key={i} style={styles.toolRow}>
              {row.map((tool) => (
                <AnimatedToolCard
                  key={tool.id}
                  tool={tool}
                  onPress={() => handleToolPress(tool)}
                />
              ))}
              {row.length === 1 && <View style={{ flex: 1 }} />}
            </View>
          ))}
        </View>
      </ScrollView>

      {/* Tool Modal */}
      <Modal
        visible={!!selectedTool}
        transparent
        animationType="slide"
        onRequestClose={() => setSelectedTool(null)}
      >
        <Pressable style={styles.modalOverlay} onPress={() => setSelectedTool(null)}>
          <Pressable style={[styles.modalSheet, { backgroundColor: theme.surface }]} onPress={() => {}}>
            <View style={styles.modalHandle} />
            {selectedTool && (
              <>
                <View style={styles.modalToolHeader}>
                  <View style={[styles.modalToolIcon, { backgroundColor: selectedTool.color + "20" }]}>
                    <ToolIcon tool={selectedTool} size={32} />
                  </View>
                  <View style={styles.modalToolInfo}>
                    <Text style={[styles.modalToolName, { color: theme.text, fontFamily: "Inter_700Bold" }]}>
                      {selectedTool.name}
                    </Text>
                    <Text style={[styles.modalToolDesc, { color: theme.textSecondary, fontFamily: "Inter_400Regular" }]}>
                      {selectedTool.description}
                    </Text>
                  </View>
                </View>
                <View style={[styles.modalDropzone, { backgroundColor: theme.surfaceSecondary }]}>
                  <Ionicons name="cloud-upload-outline" size={40} color={theme.textMuted} />
                  <Text style={[styles.modalDropText, { color: theme.textSecondary, fontFamily: "Inter_500Medium" }]}>
                    Select or drop your PDF here
                  </Text>
                  <Text style={[styles.modalDropSub, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
                    PDF files supported
                  </Text>
                </View>
                <Pressable
                  style={[styles.modalActionBtn, { backgroundColor: selectedTool.color }]}
                  onPress={() => {
                    setSelectedTool(null);
                    Alert.alert(
                      selectedTool.name,
                      "Import a PDF from your documents to use this tool.",
                      [{ text: "OK" }]
                    );
                  }}
                >
                  <Ionicons name="document-text-outline" size={20} color="#fff" />
                  <Text style={[styles.modalActionText, { fontFamily: "Inter_600SemiBold" }]}>
                    Choose from Documents
                  </Text>
                </Pressable>
              </>
            )}
          </Pressable>
        </Pressable>
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: { paddingHorizontal: 20, paddingBottom: 16 },
  title: { fontSize: 28, marginBottom: 4 },
  subtitle: { fontSize: 14 },
  categories: { paddingHorizontal: 16, gap: 8, marginBottom: 20 },
  categoryChip: { paddingHorizontal: 16, paddingVertical: 8, borderRadius: 20 },
  categoryText: { fontSize: 14 },
  toolsGrid: { paddingHorizontal: 16, gap: 12 },
  toolRow: { flexDirection: "row", gap: 12 },
  toolCard: { borderRadius: 18, padding: 16, gap: 8, flex: 1 },
  toolCardHeader: { flexDirection: "row", justifyContent: "space-between", alignItems: "flex-start" },
  toolCardIcon: { width: 46, height: 46, borderRadius: 14, alignItems: "center", justifyContent: "center" },
  badge: { paddingHorizontal: 7, paddingVertical: 3, borderRadius: 8 },
  badgeText: { color: "#fff", fontSize: 10 },
  toolCardName: { fontSize: 14 },
  toolCardDesc: { fontSize: 12, lineHeight: 16 },
  modalOverlay: { flex: 1, backgroundColor: "rgba(0,0,0,0.5)", justifyContent: "flex-end" },
  modalSheet: { borderTopLeftRadius: 24, borderTopRightRadius: 24, padding: 24, paddingBottom: 40 },
  modalHandle: { width: 40, height: 4, backgroundColor: "#ccc", borderRadius: 2, alignSelf: "center", marginBottom: 20 },
  modalToolHeader: { flexDirection: "row", gap: 14, alignItems: "center", marginBottom: 24 },
  modalToolIcon: { width: 60, height: 60, borderRadius: 18, alignItems: "center", justifyContent: "center" },
  modalToolInfo: { flex: 1 },
  modalToolName: { fontSize: 20, marginBottom: 4 },
  modalToolDesc: { fontSize: 14 },
  modalDropzone: { borderRadius: 18, padding: 32, alignItems: "center", gap: 8, marginBottom: 16, borderWidth: 2, borderStyle: "dashed", borderColor: "#CBD5E1" },
  modalDropText: { fontSize: 15 },
  modalDropSub: { fontSize: 13 },
  modalActionBtn: { borderRadius: 16, padding: 16, flexDirection: "row", alignItems: "center", justifyContent: "center", gap: 10 },
  modalActionText: { color: "#fff", fontSize: 16 },
});
